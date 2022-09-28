/************************************************************************
 * Copyright (c) 2014 Milan Jaitner                                     *
 * This program is free software: you can redistribute it and/or modify *
 * it under the terms of the GNU General Public License as published by *
 * the Free Software Foundation, either version 3 of the License, or    * 
 * any later version.													*
																		*
 * This program is distributed in the hope that it will be useful,      *
 * but WITHOUT ANY WARRANTY; without even the implied warranty of       *
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the         *
 * GNU General Public License for more details.							*
																		*
 * You should have received a copy of the GNU General Public License	*
 * along with this program.  If not, see http://www.gnu.org/licenses/	*
 ***********************************************************************/

using UnityEngine;
using System.Collections;

using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Localization manager
/// @Author - Milan Jaitner
/// </summary>

public class LocalizationFile
{
	public Dictionary<string, string> data = new Dictionary<string, string>();

	public LocalizationFile()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LocalizationFile"/> class and loads localization file.
	/// Expects file with lines with syntax "key":"value"
	/// </summary>
	/// <param name="_langFilePath">Localization file path.</param>
	public LocalizationFile(string _langFilePath)
	{
		ParseLocalization(_langFilePath);
	}

	/// <summary>
	/// Parses the localization.
	/// </summary>
	/// <param name="_langFilePath">_lang file path.</param>
	protected virtual void ParseLocalization(string _langFilePath)
	{
		data.Clear();

		TextAsset _textAsset = (TextAsset)Resources.Load(_langFilePath, typeof(TextAsset));

		if (_textAsset == null)
		{
			Debug.LogError("Error, localization file " + _langFilePath + " not found!");
			return;
		}

		Regex exp = new Regex("^\"(?<name>[^\"]+)\":\"(?<value>[^\"]+)\"$");

		using (StringReader _sr = new StringReader(_textAsset.text))
		{
			string _line;
			while ((_line = _sr.ReadLine()) != null)
			{
				Match _m = exp.Match(_line);

				if (_m.Success) 
				{
					string key = _m.Groups["name"].Value;
					string value = _m.Groups["value"].Value;

					if (!string.IsNullOrEmpty (value))
						value = value.Replace("\\n", System.Environment.NewLine);

					if (data.ContainsKey (key)) 
					{
						Debug.LogError ("key: " + key + " is in localization file twice or more.");
					} 
					else 
					{
						data.Add(key, value);
					}
				}
			}
		}
		//Debug.Log("Loaded localization " + _langFilePath);
	}

	/// <summary>
	/// Gets value from localization by key
	/// </summary>
	/// <returns>Localized value</returns>
	/// <param name="_key">Key</param>
	public virtual string GetValue(string _key)
	{
		return data.ContainsKey(_key) ? data[_key] : null;
	}
}

public class Localization : MonoSingletonPersistent<Localization>
{
	protected const string localizationPath = "Localizations";
	protected LocalizationFile defaultLocalization;
	protected new LocalizationFile localization;

	private string _langID = "en";

	public string LangID
	{
		get
		{
			return this._langID;
		}
		set
		{
			_langID = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();

		if (DebugConfig.Instance.forceEnglish) 
		{
			_langID = "en";
		}
		else
		{
			switch (Application.systemLanguage)
			{
				default:
				case SystemLanguage.English:
					_langID = "en";
				break;

				case SystemLanguage.Czech:
				case SystemLanguage.Slovak:
					_langID = "cs";
				break;

				case SystemLanguage.German:
					_langID = "de";
				break;

				case SystemLanguage.Russian:
					_langID = "ru";
				break;
				case SystemLanguage.Italian:
					_langID = "it";
				break;

				case SystemLanguage.Spanish:
					_langID = "es";
				break;
				case SystemLanguage.Portuguese:
					_langID = "br";
				break;

				case SystemLanguage.French:
					_langID = "fr";
				break;
				case SystemLanguage.Vietnamese:
				case SystemLanguage.Chinese:
					_langID = "zh";
				break;

				case SystemLanguage.Japanese:
					_langID = "jp";
				break;
			}
		}

		LoadLocalizations();
	}

	protected virtual void LoadLocalizations()
	{
		defaultLocalization = new LocalizationFile(localizationPath + "/en");
		localization = new LocalizationFile(localizationPath + "/" + _langID);

		//Debug.Log("Setting language: " + _langID);
	}

	/// <summary>
	/// Gets value from localization by key
	/// </summary>
	/// <returns>Localized value</returns>
	/// <param name="_key">Key</param>
	public virtual string GetValue(string _key, params object[] p)
	{
#if UNITY_EDITOR
		if (defaultLocalization == null)
			LoadLocalizations();
#endif
		if (defaultLocalization == null || localization == null)
			return null;

		string _value = localization.GetValue(_key);

		if (_value == null)
		{
			_value = defaultLocalization.GetValue(_key);

			if (_value == null)
			{
				return _key;
			}
		}

		if(_value != null && p.Length > 0)
		{
			_value = string.Format(_value, p);
		}

		return _value;
	}

	public virtual bool HasValue(string key)
	{
		#if UNITY_EDITOR
		if (defaultLocalization == null)
			LoadLocalizations();
		#endif
		if (defaultLocalization == null || localization == null)
			return false;
		
		string value = localization.GetValue(key);

		if (value == null)
		{
			value = defaultLocalization.GetValue(key);

			if (value == null)
			{
				return false;
			}
		}

		return true;
	}
}