using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

public class UserController : ApiController
{
    private UserContext UserContext = new UserContext();

    #region Users

    [HttpGet]
    [Route("Users")]
    public IHttpActionResult Get()
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;
        User user;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {
            user = UserContext.Get(userKey);

            if (user != null)
                return Ok(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.ImageURL,
                    user.Status,
                    user.ThemeId
                });
            else BadRequest();
        }

        return Ok("Unauthorized");
    }

    [HttpGet]
    [Route("Users({id})")]
    public IHttpActionResult Get(int id)
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;
        User user;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {
            user = UserContext.Get(id);

            if (user != null)
                return Ok(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.ImageURL,
                    user.Status,
                    user.ThemeId
                });
            else BadRequest();
        }

        return Ok("Unauthorized");
    }

    [HttpPost]
    [Route("Users/Login")]
    public IHttpActionResult Login(JObject json)
    {
        var email = json["Email"]?.ToString();
        var password = json["Password"]?.ToString();

        var user = UserContext.Login(email, password);

        if (user != null)
        {
            var sessionToken = UserContext.IniciateUserSession(user.Id);
            var locations = UserContext.Getlocations(user.Id);


            if(locations.Count() > 1)
            {
                return Ok(new
                {
                    SessionToken = sessionToken,
                    User = new
                    {
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        Home = new
                        {
                            locations.First().Latitude,
                            locations.First().Longitude
                        },
                        Work = new
                        {
                            locations.Last().Latitude,
                            locations.Last().Longitude
                        },
                        user.ImageURL,
                        user.Status,
                        user.ThemeId
                    }
                });
            }
            else
            {

                return Ok(new
                {
                    SessionToken = sessionToken,
                    User = new
                    {
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        Home = new
                        {
                            locations.First().Latitude,
                            locations.First().Longitude
                        },
                        user.ImageURL,
                        user.Status,
                        user.ThemeId
                    }
                });
            }
        }
        else
            return Ok("Unauthorized");
    }

    [HttpGet]
    [Route("Users/Logout")]
    public IHttpActionResult Logout()
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;
        User user;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {
            user = UserContext.Get(userKey);

            return Ok(UserContext.Logout(user.Id));
        }

        return Ok("Unauthorized");
    }

    [HttpPost]
    [Route("Users/Register")]
    public IHttpActionResult Register(JObject json)
    {
        var email = json["Email"]?.ToString();
        var firstName = json["FirstName"]?.ToString();
        var lastName = json["LastName"]?.ToString();
        var password = json["Password"]?.ToString();
        var status = json["Status"]?.ToString();
        if(!int.TryParse(json["ThemeId"]?.ToString(), out int themeId))
            themeId = 0;
        Location home = JsonConvert.DeserializeObject<Location>(json["Home"]?.ToString());
        Location work = JsonConvert.DeserializeObject<Location>(json["Work"] != null ? json["Work"].ToString() : "");

        bool success = false;

        if (!IsValidEmail(email))
            return BadRequest("Not valid email.");

        if (!string.IsNullOrEmpty(firstName) && 
            !string.IsNullOrEmpty(password) && 
            !string.IsNullOrEmpty(lastName) &&
            home != null)
            success = UserContext.Register(email, password, firstName, lastName, status, themeId, home, work);
        else
            return BadRequest("Please fill in all fields.");

        if (success)
        {
            var user = UserContext.Login(email, password);
            var sessionToken = UserContext.IniciateUserSession(user.Id);
            var locations = UserContext.Getlocations(user.Id);


            if(locations.Count() > 1)
            {
                return Ok(new
                {
                    Success = success,
                    SessionToken = sessionToken,
                    User = new
                    {
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        Home = new
                        {
                            locations.First().Latitude,
                            locations.First().Longitude
                        },
                        Work = new
                        {
                            locations.Last().Latitude,
                            locations.Last().Longitude
                        },
                        user.ImageURL,
                        user.Status,
                        user.ThemeId
                    }
                });
            }
            else
            {

                return Ok(new
                {
                    Success = success,
                    SessionToken = sessionToken,
                    User = new
                    {
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        Home = new
                        {
                            locations.First().Latitude,
                            locations.First().Longitude
                        },
                        user.ImageURL,
                        user.Status,
                        user.ThemeId
                    }
                });
            }
        }
        else
            return Ok(new
            {
                Success = success
            });
    }

    [HttpPost]
    [Route("Users")]
    public IHttpActionResult Update(JObject json)
    {
        


        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;
        User user;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {
            user = UserContext.Get(userKey);

            var firstName = json["FirstName"]?.ToString();
            var lastName = json["LastName"]?.ToString();
            var status = json["Status"]?.ToString();
            int.TryParse(json["Theme"]?.ToString(), out int themeId);
            Location home = JsonConvert.DeserializeObject<Location>(json["Home"] != null ? json["Home"].ToString() : "");
            Location work = JsonConvert.DeserializeObject<Location>(json["Work"] != null ? json["Work"].ToString() : "");

            bool success = false;

            success = UserContext.Update(user.Id, firstName, lastName, status, themeId, home, work);

            if (success)
            {
                var locations = UserContext.Getlocations(user.Id);


                if (locations.Count() > 1)
                {
                    return Ok(new
                    {
                        Success = success,
                        User = new
                        {
                            user.Id,
                            user.FirstName,
                            user.LastName,
                            Home = new
                            {
                                locations.First().Latitude,
                                locations.First().Longitude
                            },
                            Work = new
                            {
                                locations.Last().Latitude,
                                locations.Last().Longitude
                            },
                            user.ImageURL,
                            user.Status,
                            user.ThemeId
                        }
                    });
                }
                else
                {

                    return Ok(new
                    {
                        Success = success,
                        User = new
                        {
                            user.Id,
                            user.FirstName,
                            user.LastName,
                            Home = new
                            {
                                locations.First().Latitude,
                                locations.First().Longitude
                            },
                            user.ImageURL,
                            user.Status,
                            user.ThemeId
                        }
                    });
                }
            }
            else
                return Ok(new
                {
                    Success = success
                });
        }

        return Ok("Unauthorized");
    }

    [HttpDelete]
    [Route("Users")]
    public IHttpActionResult Delete()
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;
        User user;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {
            user = UserContext.Get(userKey);

            return Ok(UserContext.Delete(user.Id));
        }

        return Ok("Unauthorized");
    }

    [HttpPost]
    [Route("Users/Image")]
    public IHttpActionResult Image()
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;
        User user;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {
            var httpRequest = HttpContext.Current.Request;
            var postedFile = httpRequest.Files.Count > 0 ? httpRequest.Files[0] : null;

            if (postedFile != null && postedFile.ContentLength > 0)
            {
                IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                var extension = ext.ToLower();
                if (!AllowedFileExtensions.Contains(extension))
                {
                    var message = string.Format("Please Upload image of type .jpg,.gif,.png.");

                    return Ok(new
                    {
                        Success = false,
                        Message = message
                    });
                }
                else
                {
                    user = UserContext.Get(userKey);

                    var response = UserContext.UploadUserPhoto(postedFile, user.ImageURL);

                    if (response)
                        return Ok(new
                        {
                            Success = true,
                            user.ImageURL
                        });
                    else
                        return Ok(new
                        {
                            Success = false
                        });
                }
            }
            else
                return Ok(new
                {
                    Success = false,
                    Message = "No files sent"
                });
        }

        return Ok("Unauthorized");
    }

    #endregion

    #region UserTheme

    [HttpGet]
    [Route("Users/Themes")]
    public IHttpActionResult GetThemes()
    {
        string userKey = this.Request.Headers.GetValues("uk").FirstOrDefault();
        bool auth = false;

        using (UserContext userContext = new UserContext())
        {
            auth = userContext.Authenticate(userKey);
        }

        if (auth)
        {
            List<UserTheme> eventTypes = UserContext.GetThemes();

            return Ok(eventTypes);
        }

        return Ok("Unauthorized");
    }

    #endregion

    #region Other Functions
    bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}