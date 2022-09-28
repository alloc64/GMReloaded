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
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using TouchOrchestra;

namespace GMReloaded
{
	public class ConfigFile
	{
		protected Dictionary<string, object> data = new Dictionary<string, object>();

		public ConfigFile(string path)
		{
			ParseFile(path);
		}

		protected virtual void ParseFile(string _filePath)
		{
			data.Clear();

			TextAsset _textAsset = (TextAsset)Resources.Load(_filePath, typeof(TextAsset));

			if(_textAsset == null)
			{
				Debug.LogError("Error, config file " + _filePath + " not found!");
				return;
			}

			Regex _inlineNotationRegex = new Regex("^\"(?<name>[^\"]+)\":\"(?<value>[^\"]+)\"$");
			Regex _objectNotationRegex = new Regex("^\"(?<name>[^\"]+)\":$");

			bool _multilineComment = false;

			using(StringReader _sr = new StringReader(_textAsset.text))
			{
				string _line;
				while((_line = _sr.ReadLine()) != null)
				{
					Match _match = _inlineNotationRegex.Match(_line);

					if(_match.Success)
					{
						string _key = _match.Groups["name"].Value;
						string _value = _match.Groups["value"].Value;

						object _outValue = "";
						if(data.TryGetValue(_key, out _outValue))
						{
							Debug.LogError("Error, key " + _key + " already exists!");
							continue;
						}

						data.Add(_key, _value);
					} 
					else
					{
						if(_line == "" || _line == " " || _line == "\t" || _line.StartsWith("\\") || _line.StartsWith("//"))
							continue;

						if(_line.StartsWith("/*") || _line.EndsWith("/*"))
						{
							_multilineComment = true;
						}

						if(_line.StartsWith("*/") || _line.EndsWith("*/"))
						{
							_multilineComment = false;
						}

						if(_multilineComment)
							continue;

						_match = _objectNotationRegex.Match(_line);

						if(_match.Success)
						{
							string _jsonOBJ = "";
							string _objectID = _match.Groups["name"].Value;

							while((_line = _sr.ReadLine()) != null)
							{
								_line = _line.Trim();

								if(_line == "" || _line == " " || _line == "\t" || _line.StartsWith("\\") || _line.StartsWith("//"))
									continue;

								_jsonOBJ += _line + "\n";

								if(_line == "}")
									break;
							}

							data.Add(_objectID, new JSONObject(_jsonOBJ));
						}
					}
				}
			}
		}

		public virtual string GetValue(string _key, object _param = null)
		{
			if(data.ContainsKey(_key))
			{
				object _value = data[_key];

				if(_value is string)
				{
					string _string = (string)data[_key];

					return _param == null ? _string : string.Format(_string, _param);
				}
			} 

			return null;
		}

		public virtual JSONObject GetJSONObject(string _key)
		{
			if(data.ContainsKey(_key))
			{
				object _value = data[_key];

				if(_value is JSONObject)
				{
					return (JSONObject)_value;
				}
			}

			return null;
		}
	}
}
