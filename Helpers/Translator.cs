﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ff14bot.Enums;
using ff14bot.Managers;

namespace LlamaLibrary.Helpers
{
    public static class Translator
    {
        public static readonly Language Language;

        public static string SummoningBell => summoning_Bell[Language];

        public static string VentureCompleteText => addon2385[Language];

        public static string AssignVentureText => addon2386[Language];

        public static string AssignVentureInProgressText => addon2387[Language];

        public static string SellInventory => addon2380[Language];

        public static string SellRetainer => addon2381[Language];

        public static string EntrustRetainer => addon2378[Language];

        static Translator()
        {
            Language = (Language)typeof(DataManager).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .First(i => i.FieldType == typeof(Language)).GetValue(null);
        }

        //Addon # 2378
        private static Dictionary<Language, string> addon2378 = new Dictionary<Language, string>
        {
            { Language.Eng, "Entrust or withdraw items." },
            { Language.Jap, "アイテムの受け渡し" },
            { Language.Fre, "Échanger des objets" },
            { Language.Ger, "Gegenstände geben oder nehmen" },
            { Language.Chn, "" }
        };

        private static Dictionary<Language, string> summoning_Bell = new Dictionary<Language, string>
        {
            { Language.Eng, "Summoning Bell" },
            { Language.Jap, "リテイナーベル" },
            { Language.Fre, "Sonnette" },
            { Language.Ger, "Krämerklingel" },
            { Language.Chn, "传唤铃" }
        };

        //Addon # 2385
        private static Dictionary<Language, string> addon2385 = new Dictionary<Language, string>
        {
            { Language.Eng, "View venture report. (Complete)" },
            { Language.Jap, "リテイナーベンチャーの確認　[完了]" },
            { Language.Fre, "Voir le rapport de la tâche terminée" },
            { Language.Ger, "Abgeschlossene Unternehmung einsehen" },
            { Language.Chn, "查看雇员探险情况　[结束]" }
        };

        //Addon # 2386
        private static Dictionary<Language, string> addon2386 = new Dictionary<Language, string>
        {
            { Language.Eng, "Assign venture." },
            { Language.Jap, "リテイナーベンチャーの依頼" },
            { Language.Fre, "Liste des tâches" },
            { Language.Ger, "Mit Unternehmung beauftragen" },
            { Language.Chn, "委托雇员进行探险" }
        };

        //Addon # 2387
        private static Dictionary<Language, string> addon2387 = new Dictionary<Language, string>
        {
            { Language.Eng, "Assign venture. (In progress)" },
            { Language.Jap, "リテイナーベンチャーの依頼　[依頼中]" },
            { Language.Fre, "Liste des tâches [Tâche en cours]" },
            { Language.Ger, "Mit Unternehmung beauftragen (Gehilfe beschäftigt)" },
            { Language.Chn, "委托雇员进行探险　[进行中]" }
        };

        //Addon # 12590
        private static Dictionary<Language, string> addon12590 = new Dictionary<Language, string>
        {
            { Language.Eng, "None in progress" },
            { Language.Jap, "依頼なし" },
            { Language.Fre, "Aucune" },
            { Language.Ger, "Keine Unternehmung" },
            { Language.Chn, "没有探险委托" }
        };

        //Addon # 12591
        private static Dictionary<Language, string> addon12591 = new Dictionary<Language, string>
        {
            { Language.Eng, "Complete in " },
            { Language.Jap, "残り時間" },
            { Language.Fre, "Fin de la tâche dans " },
            { Language.Ger, "Noch " },
            { Language.Chn, "剩余时间" }
        };

        //Addon # 12592
        private static Dictionary<Language, string> addon12592 = new Dictionary<Language, string>
        {
            { Language.Eng, "Complete" },
            { Language.Jap, "完了" },
            { Language.Fre, "Terminée" },
            { Language.Ger, "Abgeschlossen" },
            { Language.Chn, "结束" }
        };

        //Addon # 2380
        private static Dictionary<Language, string> addon2380 = new Dictionary<Language, string>
        {
            { Language.Eng, "Sell items in your inventory on the market." },
            { Language.Jap, "マーケット出品（プレイヤー所持品から）" },
            { Language.Fre, "Mettre en vente un objet de votre inventaire" },
            { Language.Ger, "Gegenstände aus dem eigenen Inventar verkaufen" },
            { Language.Chn, "" }
        };

        //Addon # 2381
        private static Dictionary<Language, string> addon2381 = new Dictionary<Language, string>
        {
            { Language.Eng, "Sell items in your retainer's inventory on the market." },
            { Language.Jap, "マーケット出品（リテイナー所持品から）" },
            { Language.Fre, "Mettre en vente un objet du servant" },
            { Language.Ger, "Gegenstände aus dem Gehilfeninventar verkaufen" },
            { Language.Chn, "" }
        };
    }
}