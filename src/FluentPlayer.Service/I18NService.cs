using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Magentaize.FluentPlayer.Core;
using Magentaize.FluentPlayer.Core.IO;

namespace Magentaize.FluentPlayer.Service
{
    public class I18NService
    {
        private readonly string _builtinLanguagesDirectory = Path.Combine(ApplicationPaths.ExecutionFolder, ApplicationPaths.BuiltinLanguagesFolder);
        private List<I18NLanguage> _languages;
        private I18NLanguage _defaultLanguage;

        public I18NService()
        {
            LoadLanguages();
        }

        public void ApplyLanguage(string code)
        {
                var selectedLanguage = _languages.Where((l) => l.Code.ToUpper().Equals(code.ToUpper())).Select((l) => l).FirstOrDefault();

                if (selectedLanguage == null)
                {
                    selectedLanguage = GetDefaultLanguage();
                }

                var dict = new Dictionary<string, string>();

                foreach (var text in _defaultLanguage.Texts)
                {
                    dict.Add(text.Key, text.Value);
                }
                var dis = new DisplayLanguage(dict);

            Application.Current.Resources["DisplayLanguage"] = dis;
        }

        public List<I18NLanguage> GetLanguages()
        {
            return _languages.OrderBy((l) => l.Name).ToList();
        }

        public I18NLanguage GetLanguage(string code)
        {

            return _languages.Where((l) => l.Code.ToUpper().Equals(code.ToUpper())).Select((l) => l).FirstOrDefault();
        }

        public I18NLanguage GetDefaultLanguage()
        {

            return _defaultLanguage;
        }

        private I18NLanguage CreateLanguage(string languageFile)
        {
            var xdoc = XDocument.Load(languageFile);

            I18NLanguage returnLanguage = null;

            var languageInfo = (from t in xdoc.Elements("Language")
                                select t).FirstOrDefault();

            if (languageInfo != null)
            {
                returnLanguage = new I18NLanguage(languageInfo.Attribute("Code").Value,
                    languageInfo.Attribute("Name").Value);

                var textElements = (from t in xdoc.Element("Language").Elements("Text")
                                    select t).ToList();

                var texts = new Dictionary<string, string>();

                foreach (var element in textElements)
                {
                    if (!texts.ContainsKey(element.Attribute("Key").Value))
                    {
                        texts.Add(element.Attribute("Key").Value, element.Value);
                    }
                }

                returnLanguage.Texts = texts;
            }

            return returnLanguage;
        }

        private void LoadLanguages()
        {
            if (_languages == null)
            {
                _languages = new List<I18NLanguage>();
            }
            else
            {
                _languages.Clear();
            }

            var builtinLanguageFiles = Directory.GetFiles(_builtinLanguagesDirectory, "*.xml");

            foreach (var builtinLanguageFile in builtinLanguageFiles)
            {
                var builtinlanguage = CreateLanguage(builtinLanguageFile);

                if (builtinlanguage != null)
                {
                    if (builtinlanguage.Code.ToUpper().Equals(Defaults.DefaultLanguageCode))
                    {
                        _defaultLanguage = builtinlanguage;
                    }

                    // This makes sure custom languages have a higher priority than builtin languages.
                    //  This allows the user to customize.
                    if (!_languages.Contains(builtinlanguage))
                    {
                        _languages.Add(builtinlanguage);
                    }
                }
            }
        }

        private string GetTextValue(I18NLanguage language, string key)
        {
            if (language.Texts.ContainsKey(key) && !string.IsNullOrEmpty(language.Texts[key]))
            {
                // If the key can be found in the selected language, return that
                return language.Texts[key];
            }
            else
            {
                // Otherwise, return the key from the default language
                return _defaultLanguage.Texts[key];
            }
        }
    }
}