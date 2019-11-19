using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using System.IO;
using System.Drawing;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Drawing.Imaging;
using System.Configuration;

public class UserContext : DbContext
{
    public UserContext() : base("name=JoinDB") { }

    public DbSet<User> User { get; set; }
    public DbSet<UserSession> UserSession { get; set; }
    public DbSet<UserLocation> UserLocation { get; set; }
    public DbSet<UserTheme> UserTheme { get; set; }

    private string JoinStorageCS = ConfigurationManager.ConnectionStrings["JoinStorage"].ConnectionString;

    #region Users

    public bool Register(string email, string password, string firstName, string lastName, string status, int themeId, Location home, Location work = null)
    {
        if (User.Where(s => s.Email == email).Count() > 0)
        {
            return false;
        }
        else
        {
            UserLocation homeLoc = new UserLocation
            {
                IsHome = true,
                IsWork = false,
                Longitude = home.Longitude,
                Latitude = home.Latitude
            };

            homeLoc = UserLocation.Add(homeLoc);

            SaveChanges();

            UserLocation workLoc = null;

            if(work != null)
            {
                workLoc = new UserLocation
                {
                    IsHome = false,
                    IsWork = true,
                    Longitude = work.Longitude,
                    Latitude = work.Latitude
                };

                workLoc = UserLocation.Add(workLoc);

                SaveChanges();
            }

            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string base64Salt = Convert.ToBase64String(salt);

            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            string image = Convert.ToBase64String(salt);

            SetDefaultUserPhoto(image + ".png");

            var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = hashedPassword,
                Salt = base64Salt,
                ImageURL = image + ".png",
                Status = !string.IsNullOrEmpty(status) ? status : "Online",
                HomeId = homeLoc.Id,
                WorkId = workLoc != null ? workLoc.Id : 0,
                ThemeId = (themeId != 0 && UserTheme.Where(w => w.Id == themeId).Count() > 0) ? themeId : 1
            };

            User.Add(newUser);

            SaveChanges();

            return true;
        }
    }

    public bool Update(int userId, string firstName = "", string lastName = "", string status = "", int themeId = 0, Location home = null, Location work = null)
    {

        if (User.Where(s => s.Id == userId).Count() > 0)
        {
            User userUpdated = User.Where(w => w.Id == userId).First();

            userUpdated.FirstName = !string.IsNullOrEmpty(firstName) ? firstName : userUpdated.FirstName;
            userUpdated.LastName = !string.IsNullOrEmpty(lastName) ? lastName : userUpdated.LastName;
            userUpdated.Status = !string.IsNullOrEmpty(status) ? status : userUpdated.Status;
            userUpdated.ThemeId = (themeId != 0 && UserTheme.Where(w => w.Id == themeId).Count() > 0) ? themeId : userUpdated.ThemeId;

            SaveChanges();

            if (home != null)
            {
                UserLocation homeUpdated = UserLocation.Where(w => w.Id == userUpdated.HomeId).First();

                homeUpdated.Latitude = home.Latitude;
                homeUpdated.Longitude = home.Longitude;

                SaveChanges();

            }
            if(work != null)
            {
                if(userUpdated.WorkId != 0)
                {
                    UserLocation workUpdated = UserLocation.Where(w => w.Id == userUpdated.WorkId).First();

                    workUpdated.Latitude = home.Latitude;
                    workUpdated.Longitude = home.Longitude;

                    SaveChanges();
                }
                else
                {
                    UserLocation workLoc = new UserLocation
                    {
                        IsHome = true,
                        IsWork = false,
                        Longitude = home.Longitude,
                        Latitude = home.Latitude
                    };

                    workLoc = UserLocation.Add(workLoc);

                    SaveChanges();

                    userUpdated.WorkId = workLoc.Id;

                    SaveChanges();
                }
            }

            return true;
        }


        return true;
    }

    public bool Logout(int userId)
    {
        if(UserSession.Where(s => s.UserId == userId).Count() > 0)
        {
            UserSession deletedUserSession = UserSession.Where(s => s.UserId == userId).FirstOrDefault();

            UserSession.Remove(deletedUserSession);

            SaveChanges();

            return true;
        }

        return false;
    }

    public bool Delete(int userId)
    {
        if (User.Where(s => s.Id == userId).Count() > 0)
        {
            UserSession deletedSession = UserSession.Where(w => w.UserId == userId).FirstOrDefault();

            UserSession.Remove(deletedSession);

            SaveChanges();

            User deletedUser = User.Where(w => w.Id == userId).FirstOrDefault();

            User.Remove(deletedUser);

            SaveChanges();

            return true;
        }
        return false;
    }

    public User Get(string sessionToken)
    {
        var session = UserSession.Where(w => w.SessionToken == sessionToken).FirstOrDefault();
        if (session != null)
        {
            User user = User.Where(w => w.Id == session.UserId).FirstOrDefault();
            return user;
        }
        return null;
    }

    public User Get(int id)
    {
        User user = User.Where(w => w.Id == id).FirstOrDefault();
        if (user != null) return user;
        else return null;
    }

    public List<UserLocation> Getlocations(int id)
    {
        User user = User.Where(w => w.Id == id).FirstOrDefault();

        UserLocation home = UserLocation.Where(w => w.Id == user.HomeId).FirstOrDefault();

        UserLocation work = UserLocation.Where(w => w.Id == user.WorkId).FirstOrDefault();

        var locations = new List<UserLocation>();

        locations.Add(home);

        if(work != null)
            locations.Add(work);

        return locations;
    }

    public User Login(string email, string password)
    {
        try
        {
            var user = User.Where(w => w.Email == email).FirstOrDefault();

            if (user != null)
            {
                var decodedSalt = Convert.FromBase64String(user.Salt);

                string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: decodedSalt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));

                if (user.Password != hashedPassword)
                    return null;
            }

            return user;
        }
        catch (Exception e)
        {
            throw new Exception($"Deu Ruim {e.Message}");
        }
    }

    public string IniciateUserSession(int userId)
    {
        var session = UserSession.Where(w => w.UserId == userId).FirstOrDefault();
        string sessionToken;

        if (session == null)
        {
            sessionToken = GenerateSessionToken();

            UserSession newUserSession = new UserSession
            {
                UserId = userId,
                SessionToken = sessionToken,
                ExpireDate = DateTime.Now.AddDays(1)
            };

            UserSession.Add(newUserSession);

            SaveChanges();
        }
        else
            sessionToken = session.SessionToken;

        return sessionToken;
    }

    private void SetDefaultUserPhoto(string userImgUrl)
    {
        var path = AppDomain.CurrentDomain.BaseDirectory + @"tmp\JoinUser.png";

        CloudStorageAccount account = CloudStorageAccount.Parse(JoinStorageCS);
        CloudBlobClient serviceClient = account.CreateCloudBlobClient();

        // Create container. Name must be lower case.
        var container = serviceClient.GetContainerReference("images");
        container.CreateIfNotExistsAsync().Wait();

        CloudBlockBlob blob = container.GetBlockBlobReference(userImgUrl);
        blob.UploadFromFileAsync(path).Wait();

        File.Delete(path);
    }

    public bool UploadUserPhoto(HttpPostedFile file, string userImgUrl)
    {
        try
        {
            Stream strm = file.InputStream;

            Bitmap bitmap;

            using (Image img = Image.FromStream(strm))
            {
                int width = img.Width < 150 ? img.Width : 150;
                int height = img.Height < 150 ? img.Height : 150;

                bitmap = new Bitmap(img, width, height);
            }
            var path = AppDomain.CurrentDomain.BaseDirectory + @"tmp\" + userImgUrl;

            bitmap.Save(path);

            CloudStorageAccount account = CloudStorageAccount.Parse(JoinStorageCS);
            CloudBlobClient serviceClient = account.CreateCloudBlobClient();

            // Create container. Name must be lower case.
            var container = serviceClient.GetContainerReference("images");
            container.CreateIfNotExistsAsync().Wait();

            CloudBlockBlob blob = container.GetBlockBlobReference(userImgUrl);
            blob.UploadFromFileAsync(path).Wait();

            File.Delete(path);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
        
    }

    public bool Authenticate(string userKey)
    {
        if (UserSession.Where(w => w.SessionToken == userKey).Count() > 0)
            return true;
        return false;
    }

    public List<UserTheme> GetThemes()
    {
        return UserTheme.Select(s => s).ToList();
    }

    #endregion

    #region Other Functions

    private string GenerateSessionToken()
    {
        string date = DateTime.Now.ToString();

        StringBuilder Sb = new StringBuilder();

        using (SHA256 hash = SHA256Managed.Create())
        {
            Encoding enc = Encoding.UTF8;
            Byte[] result = hash.ComputeHash(enc.GetBytes(date));

            foreach (Byte b in result)
                Sb.Append(b.ToString("x2"));
        }
        return Sb.ToString();
    }

    #endregion
}