using System;
using System.Net;
using System.Net.Http;
using Twilio.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Mvc;
using System.Web.Mvc;

namespace WebShutsuganCallCenter.Controllers
{
    public class MyTwilioController : TwilioController
    {
        private readonly object SAY_ATTRIBUTES = new { voice = "woman", language = "ja-jp" };

        [HttpPost]
        public TwiMLResult IncomingCall(TwilioRequest twilioRequest)
        {
            var twilioResponse = new TwilioResponse();

            twilioResponse
                .BeginGather(
                    new
                    {
                        action = "/HandleUserInput.xml",
                        numDigits = 1
                    })
                    .Say("こちらは、うぇぶしゅつがんこーるせんたーです。", SAY_ATTRIBUTES)
                    .Say("こーるせんたーのうけつけじかんにかんするおといあわせは、いちを", SAY_ATTRIBUTES)
                    .Say("うぇぶしゅつがんのそうさほうほうにかんするおといあわせは、にを", SAY_ATTRIBUTES)
                    .Say("にゅうしせいどにかんするおといあわせは、さんを押してください。", SAY_ATTRIBUTES)
                .EndGather()
                .Say("おそれいりますが、もういちどそうさしてください", SAY_ATTRIBUTES)
                .Redirect("/IncomingCall.xml");

            //return Request.CreateResponse(HttpStatusCode.OK, twilioResponse.Element);
            return new TwiMLResult(twilioResponse);

        }

        [HttpPost]
        public TwiMLResult HandleUserInput(TwilioRequest twilioRequest)
        {
            
            var userPushed = int.Parse(Request["Digits"].ToString());
            var twilioResponse = new TwilioResponse();
            switch (userPushed)
            {
                case 1:
                    twilioResponse.Say("こーるせんたーのうけつけじかんは、へいじつのごぜん くじから、ごご ごじまでです。");
                    break;
                case 2:
                    twilioResponse
                        .Say("おぺれーたーにおつなぎします。しばらくおまちください。")
                        .Dial(new Number("+81 90-3949-7949"));
                    break;
                case 3:
                    twilioResponse
                        .Say("にゅうがくせんたーにおつなぎします。しばらくおまちください。")
                        .Dial(new Number("+81 90-5898-2477"));
                    break;
                default:
                    throw new ApplicationException();
            }

            return new TwiMLResult(twilioResponse);

        }

    }
}
