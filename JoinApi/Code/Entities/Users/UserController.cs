using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

public class UserController : ApiController
{
    private UserContext UserContext = new UserContext();

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

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName
            });
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

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName
            });
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

            return Ok(new
            {
                SessionToken = sessionToken,
                User = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName
                }
            });
        }
        else
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
        var faceId = json["FaceId"]?.ToString() != null ? json["FaceId"].ToString() : "";
        var googleId = json["GoogleId"]?.ToString() != null ? json["GoogleId"].ToString() : "";

        bool sucess = false;

        if (!IsValidEmail(email))
            return BadRequest("Not valid email.");

        if (firstName != null && firstName != "" && password != null && password != "" && lastName != null && lastName != "")
            sucess = UserContext.Register(email, password, firstName, lastName, faceId, googleId);
        else
            return BadRequest("Please fill in all fields.");

        if (sucess)
        {
            var user = UserContext.Login(email, password);
            var sessionToken = UserContext.IniciateUserSession(user.Id);

            return Ok(new
            {
                Sucess = sucess,
                SessionToken = sessionToken,
                User = new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName
                }
            });
        }
        else
            return Ok(new
            {
                Sucess = sucess
            });
    }

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
}