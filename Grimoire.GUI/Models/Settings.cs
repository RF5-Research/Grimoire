﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Grimoire.GUI.Models
{
    public class Settings
    {
        public static readonly Dictionary<SystemLanguage, string> GameLanguages = new()
        {
            { SystemLanguage.English, "Eng/" },
			{ SystemLanguage.German, "Ger/" },
			{ SystemLanguage.French, "Fre/" },
		};

		public enum SystemLanguage
		{
			Afrikaans = 0,
			Arabic = 1,
			Basque = 2,
			Belarusian = 3,
			Bulgarian = 4,
			Catalan = 5,
			Chinese = 6,
			Czech = 7,
			Danish = 8,
			Dutch = 9,
			English = 10,
			Estonian = 11,
			Faroese = 12,
			Finnish = 13,
			French = 14,
			German = 15,
			Greek = 16,
			Hebrew = 17,
			Hungarian = 18,
			Icelandic = 19,
			Indonesian = 20,
			Italian = 21,
			Japanese = 22,
			Korean = 23,
			Latvian = 24,
			Lithuanian = 25,
			Norwegian = 26,
			Polish = 27,
			Portuguese = 28,
			Romanian = 29,
			Russian = 30,
			SerboCroatian = 31,
			Slovak = 32,
			Slovenian = 33,
			Spanish = 34,
			Swedish = 35,
			Thai = 36,
			Turkish = 37,
			Ukrainian = 38,
			Vietnamese = 39,
			ChineseSimplified = 40,
			ChineseTraditional = 41,
			Unknown = 42
		}

		public bool LoadLastProject { get; set; }
        public List<Project> Projects { get; set; }

        public Settings()
        {
            Projects = new();
        }
    }
}
