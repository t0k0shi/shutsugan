using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio.Mvc;
using Twilio.TwiML;
using Twilio.TwiML.Mvc;

namespace WebShutsuganCallCenter.Controllers
{
    public class CallController : TwilioController
    {
        private static string RESULT_KEY = "CallController_Result";
        private readonly object SAY_ATTRIBUTES = new { voice = "woman", language = "ja-jp" };
        private List<JukenResult> _items = new List<JukenResult>
            {
                new JukenResult{JukenNo="12345", Sibou="一般入試、文学部、英文学科", Result="合格"},
                new JukenResult{JukenNo="23456", Sibou="公募推薦入試、法学部、法学科", Result="不合格"},
                new JukenResult{JukenNo="34567", Sibou="センター利用入試、社会学部、社会学科", Result="合格"}
            };


        /// <summary>
        /// エントリポイント
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult IncomingCall(VoiceRequest request)
        {
            var response = new TwilioResponse();

            response
                .BeginGather(
                    new
                    {
                        action = "/Call/SelectDemo",
                        numDigits = 1
                    })
                    .Say("こちらはツジモトアプリのテスト窓口です。", SAY_ATTRIBUTES)
                    .Say("Web出願コールセンターのデモは、1 を", SAY_ATTRIBUTES)
                    .Say("合否通知のデモは、2 を押して、最後にシャープを押してください。", SAY_ATTRIBUTES)
                .EndGather()
                .Say("恐れ入りますが、もう一度操作してください。", SAY_ATTRIBUTES)
                .Redirect("/Call/IncomingCall");

            return new TwiMLResult(response);
        }


        /// <summary>
        /// デモを振り分ける
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SelectDemo(VoiceRequest request)
        {
            var response = new TwilioResponse();
            var userPushed = request.Digits;
            switch (userPushed)
            {
                case "1": response.Redirect("/Call/CallCenter"); break;
                case "2": response.Redirect("/Call/GouhiTuuti"); break;
                default:
                    response
                        .Say("1 か 2 を押してください。恐れ入りますが、もう一度最初から操作してください。", SAY_ATTRIBUTES)
                        .Redirect("/Call/IncomingCall");
                    break;
            }
            return new TwiMLResult(response);
        }


        /// <summary>
        /// 合否通知のエントリポイント
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GouhiTuuti(VoiceRequest request)
        {
            var response = new TwilioResponse();
            response
                .BeginGather(new { action = "/Call/InputJukenNo", numDigits = 5 })
                    .Say("こちらは、ワイディーシー大学合否結果案内センターです。", SAY_ATTRIBUTES)
                    .Say("あなたの受験番号5桁を押して、最後にシャープを押してください。", SAY_ATTRIBUTES)
                .EndGather()
                .Say("恐れ入りますが、もう一度最初から操作してください。", SAY_ATTRIBUTES)
                .Redirect("/Call/GouhiTuuti");
            return new TwiMLResult(response);
        }

        /// <summary>
        /// 合否通知で受験番号の入力を受け付ける
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult InputJukenNo(VoiceRequest request)
        {

            var response = new TwilioResponse();
            var result = _items.Where(item => item.JukenNo == request.Digits).FirstOrDefault();
            if (result == null)
            {
                response.Say("入力された受験番号は、存在しません。", SAY_ATTRIBUTES);
            }
            else
            {
                TempData[RESULT_KEY] = result;
                var jukenNo = string.Join(" ", result.JukenNo.ToArray());
                response
                    .BeginGather(new { action = "/Call/ConfirmJukenNo", numDigits = 1 })
                        .Say("あなたが受験したのは、" + result.Sibou + " の、受験番号 " + jukenNo + " ですね。", SAY_ATTRIBUTES)
                        .Say("正しければ 1 を、間違っている場合は 5 を押し、最後にシャープを押してください。", SAY_ATTRIBUTES)
                    .EndGather();
            }
            response
                .Say("恐れ入りますが、もう一度最初から操作してください。", SAY_ATTRIBUTES)
                .Redirect("/Call/GouhiTuuti");
            return new TwiMLResult(response);
        }

        /// <summary>
        /// 合否通知で結果を通知する
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ConfirmJukenNo(VoiceRequest request)
        {
            var response = new TwilioResponse();
            var userPushed = request.Digits;
            switch (userPushed)
            {
                case "1":
                    JukenResult result = TempData[RESULT_KEY] as JukenResult;
                    var jukenNo = string.Join(" ", result.JukenNo.ToArray());
                    if (result.Result == "合格")
                    {
                        response.Say("おめでとうございます。", SAY_ATTRIBUTES);
                    }
                    response.Say("受験番号 " + jukenNo + " は、" + result.Result + "です。", SAY_ATTRIBUTES);
                    break;
                default:
                    response
                        .Say("恐れ入りますが、もう一度最初から操作してください。", SAY_ATTRIBUTES)
                        .Redirect("/Call/GouhiTuuti");
                    break;
            }
            return new TwiMLResult(response);
        }

        /// <summary>
        /// コールセンターで操作を振り分ける
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CallCenter(VoiceRequest request)
        {
            var response = new TwilioResponse();

            response
                .BeginGather(
                    new
                    {
                        action = "/Call/ResponseUserCall",
                        numDigits = 1
                    })
                    .Say("こちらは、Web出願コールセンターです。", SAY_ATTRIBUTES)
                    .Say("コールセンターの受付時間についてのお問い合わせは、1 を", SAY_ATTRIBUTES)
                    .Say("Web出願の操作方法についてのお問い合わせは、2 を", SAY_ATTRIBUTES)
                    .Say("入試制度についてのお問い合わせは、3 を押して、最後にシャープを押してください。", SAY_ATTRIBUTES)
                .EndGather()
                .Say("恐れ入りますが、もう一度最初から操作してください。", SAY_ATTRIBUTES)
                .Redirect("/Call/CallCenter");

            return new TwiMLResult(response);
        }

        /// <summary>
        /// コールセンター
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ResponseUserCall(VoiceRequest request)
        {
            var userPushed = request.Digits;
            var response = new TwilioResponse();
            switch (userPushed)
            {
                case "1":
                    response
                        .Say("コールセンターの受付時間は、平日の午前9時から、午後5時までです。", SAY_ATTRIBUTES)
                        .Redirect("/Call/CallCenter");
                    break;
                case "2":
                    response
                        .Say("オペレーターにおつなぎします。しばらくお待ちください。", SAY_ATTRIBUTES)
                        .Pause(1)
                        .Say("ここで指定した電話番号に転送することができます。", SAY_ATTRIBUTES)
                        .Say("相手先に発信すると料金がかかるので、デモはここまでにしておきます。", SAY_ATTRIBUTES);
                        //.Dial(new Number("+819039497949"));
                    break;
                case "3":
                    response
                        .Say("入学センターにおつなぎします。しばらくお待ちください。", SAY_ATTRIBUTES)
                        .Pause(1)
                        .Say("ここで指定した電話番号に転送することができます。", SAY_ATTRIBUTES)
                        .Say("相手先に発信すると料金がかかるので、デモはここまでにしておきます。", SAY_ATTRIBUTES);
                        //.Dial(new Number("+819058982477"));
                    break;
                default:
                    response
                        .Say("恐れ入りますが、もう一度最初から操作してください。", SAY_ATTRIBUTES)
                        .Redirect("/Call/CallCenter");
                    break;
            }

            return new TwiMLResult(response);

        }

        private class JukenResult
        {
            public string JukenNo { get; set; }
            public string Sibou { get; set; }
            public string Result { get; set; }
        }
    }
}
