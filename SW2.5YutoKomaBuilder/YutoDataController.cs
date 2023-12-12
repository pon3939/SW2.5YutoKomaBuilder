﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SW2_5YutoKomaBuilder
{
    class YutoDataController
    {
        private YutoKomaJson yutoKoma;
        public YutoDataController()
        {
            yutoKoma = new YutoKomaJson();
        }

        public YutoKomaJson ReadYutoJson(string url)
        {
            var jsonURL = url + "&mode=json";
            var paletteURL = url + "&mode=palette&tool=bcdice";

            var json = FetchTextData(jsonURL);

            CharactorData charactorData = new CharactorData();
            charactorData = JsonConvert.DeserializeObject<CharactorData>(json);

            string paletteStr = FetchTextData(paletteURL);

            SetYutokomaJsonBasic(charactorData);
            SetBlankStatus(5);
            yutoKoma.data.SetStatus(MakeYutoKomaStatus(paletteStr));
            yutoKoma.data.SetParameter(MakeYutoKomaParameter(paletteStr));

            yutoKoma.data.externalUrl = url;
            yutoKoma.data.commands = paletteStr;

            return yutoKoma;

        }

        /// <summary>
        /// チャットパレットからステータス項目を抜き出す
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> MakeYutoKomaStatus(string palette)
        {
            Dictionary<string, string> status = new Dictionary<string, string>();
            List<string> yutoParameters = extractYutoParameters(palette);

            foreach (string param in yutoParameters)
            {
                string[] pair = param.Split('=');
                if (int.TryParse(pair[1], out int number))
                {
                    status.Add(pair[0], pair[1]);
                }
            }
            return status;
        }

        /// <summary>
        /// チャットパレットからパラメータ項目を抜き出す
        /// </summary>
        /// <returns>パラメータの</returns>
        private Dictionary<string, string> MakeYutoKomaParameter(string palette)
        {
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            List<string> yutoParameters = extractYutoParameters(palette);

            foreach(string param in yutoParameters)
            {
                string[] pair = param.Split('=');
                if (!int.TryParse(pair[1], out int number))
                {
                    parameter.Add(pair[0], pair[1]);
                }
            }
            return parameter;
        }

        /// <summary>
        /// 正規表現を使ってチャットパレットからパラメータの要素を抜き出す
        /// </summary>
        /// <param name="palette"></param>
        /// <returns></returns>
        private List<string> extractYutoParameters(string palette)
        {
            List<string> parameters = new List<string>();

            string prefix = "//";
            string suffix = "\n";
            string pattern = $@"{prefix}(.*?){suffix}";
            MatchCollection matches = Regex.Matches(palette, pattern);
            foreach(var match in matches)
            {
                parameters.Add(match.ToString());
            }

            return parameters;
        }

        private void SetYutokomaJsonBasic(CharactorData charactorData)
        {
            yutoKoma.kind = "character";
            yutoKoma.data.p_name = charactorData.playerName;
            yutoKoma.data.c_name = charactorData.characterName;
            yutoKoma.data.initiative = Convert.ToInt32(charactorData.mobilityTotal);

            yutoKoma.data.statuses.Add(new Status("HP", charactorData.hpTotal));
            yutoKoma.data.statuses.Add(new Status("MP", charactorData.mpTotal));
            yutoKoma.data.statuses.Add(new Status("防護点", charactorData.defenseTotal1Def));

            yutoKoma.data.parameters.Add(new Parameter("生命抵抗力", charactorData.vitResistTotal));
            yutoKoma.data.parameters.Add(new Parameter("精神抵抗力", charactorData.mndResistTotal));
        }

        private void SetBlankStatus(int number)
        {
            for(int i = 0; i < number; i++)
            {
                yutoKoma.data.statuses.Add(new Status());
            }
        }

        string FetchTextData(string url)
        {
            using (var webClient = new System.Net.WebClient())
            {
                // WebClientのDownloadはANSIなのでUTF8に変換
                webClient.Encoding = System.Text.Encoding.UTF8;
                return webClient.DownloadString(url);
            }
        }
    }
}