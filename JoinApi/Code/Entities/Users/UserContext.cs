using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

public class UserContext : DbContext
{
    public UserContext() : base("name=JoinDB") { }

    public DbSet<User> User { get; set; }
    public DbSet<UserSession> UserSession { get; set; }

    public bool Register(string email, string password, string firstName, string lastName, string faceId, string googleId)
    {
        if (User.Where(s => s.Email == email).Count() > 0)
        {
            return false;
        }
        else
        {
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

            var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = hashedPassword,
                Salt = base64Salt,
                FaceId = faceId,
                GoogleId = googleId
            };

            User.Add(newUser);

            SaveChanges();

            return true;
        }
    }

    public User Get(string sessionToken)
    {
        var session = UserSession.Where(w => w.SessionToken == sessionToken).FirstOrDefault();
        User user = User.Where(w => w.Id == session.UserId).FirstOrDefault();
        return user;
    }

    public User Get(int id)
    {
        User user = User.Where(w => w.Id == id).FirstOrDefault();
        return user;
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

    public bool Authenticate(string userKey)
    {
        if (UserSession.Where(w => w.SessionToken == userKey).Count() > 0)
            return true;
        return false;
    }

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
}