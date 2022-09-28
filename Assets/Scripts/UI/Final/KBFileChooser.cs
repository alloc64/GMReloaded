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

namespace GMReloaded.UI.Final
{
	using UnityEngine;
	using System;
	using System.IO;
	using System.Collections.Generic;

	/*
	File browser for selecting files or folders at runtime.
    */

	public enum FileChooserType
	{
		File,
		Directory
	}

	public class KBFileChooser
	{

		// Called when the user clicks cancel or select
		public delegate void FinishedCallback(string path);
		// Defaults to working directory
		public string CurrentDirectory
		{
			get
			{
				return m_currentDirectory;
			}
			set
			{
				SetNewDirectory(value);
				SwitchDirectoryNow();
			}
		}

		protected string m_currentDirectory;
		// Optional pattern for filtering selectable files/folders. See:
		// http://msdn.microsoft.com/en-us/library/wz42302f(v=VS.90).aspx
		// and
		// http://msdn.microsoft.com/en-us/library/6ff71z1w(v=VS.90).aspx
		public string SelectionPattern
		{
			get
			{
				return m_filePattern;
			}
			set
			{
				m_filePattern = value;
				ReadDirectoryContents();
			}
		}

		protected string m_filePattern;

		// Optional image for directories
		public Texture2D DirectoryImage
		{
			get
			{
				return m_directoryImage;
			}
			set
			{
				m_directoryImage = value;
				BuildContent();
			}
		}

		protected Texture2D m_directoryImage;

		// Optional image for files
		public Texture2D FileImage
		{
			get
			{
				return m_fileImage;
			}
			set
			{
				m_fileImage = value;
				BuildContent();
			}
		}

		protected Texture2D m_fileImage;

		// Browser type. Defaults to File, but can be set to Folder
		public FileChooserType BrowserType
		{
			get
			{
				return m_browserType;
			}
			set
			{
				m_browserType = value;
				ReadDirectoryContents();
			}
		}

		protected FileChooserType m_browserType;
		protected string m_newDirectory;
		protected string[] m_currentDirectoryParts;

		protected string[] m_files;
		protected GUIContent[] m_filesWithImages;
		protected int m_selectedFile;

		protected string[] m_nonMatchingFiles;
		protected GUIContent[] m_nonMatchingFilesWithImages;
		protected int m_selectedNonMatchingDirectory;

		protected string[] m_directories;
		protected GUIContent[] m_directoriesWithImages;
		protected int m_selectedDirectory;

		protected string[] m_nonMatchingDirectories;
		protected GUIContent[] m_nonMatchingDirectoriesWithImages;

		protected bool m_currentDirectoryMatches;

		protected GUIStyle CentredText
		{
			get
			{
				if(m_centredText == null)
				{
					m_centredText = new GUIStyle(GUI.skin.label);
					m_centredText.alignment = TextAnchor.MiddleLeft;
					m_centredText.fixedHeight = GUI.skin.button.fixedHeight;
				}
				return m_centredText;
			}
		}

		protected GUIStyle m_centredText;

		protected string m_name;

		protected Vector2 m_scrollPosition;

		protected FinishedCallback m_callback;

		private GUISkin m_guiSkin;

		// Browsers need at least a rect, name and callback
		public KBFileChooser(string name, FinishedCallback callback, GUISkin guiSkin)
		{
			m_name = name;
			m_browserType = FileChooserType.File;
			m_callback = callback;
			m_guiSkin = guiSkin;
			SetNewDirectory(Directory.GetCurrentDirectory());
			SwitchDirectoryNow();
		}

		protected void SetNewDirectory(string directory)
		{
			m_newDirectory = directory;
		}

		protected void SwitchDirectoryNow()
		{
			if(m_newDirectory == null || m_currentDirectory == m_newDirectory)
			{
				return;
			}
			m_currentDirectory = m_newDirectory;
			m_scrollPosition = Vector2.zero;
			m_selectedDirectory = m_selectedNonMatchingDirectory = m_selectedFile = -1;
			ReadDirectoryContents();
		}

		protected void ReadDirectoryContents()
		{
			if(m_currentDirectory == "/")
			{
				m_currentDirectoryParts = new string[] { "" };
				m_currentDirectoryMatches = false;
			}
			else
			{
				m_currentDirectoryParts = m_currentDirectory.Split(Path.DirectorySeparatorChar);
				if(SelectionPattern != null)
				{
					string[] generation = Directory.GetDirectories(
						                      Path.GetDirectoryName(m_currentDirectory),
						                      SelectionPattern
					                      );
					m_currentDirectoryMatches = Array.IndexOf(generation, m_currentDirectory) >= 0;
				}
				else
				{
					m_currentDirectoryMatches = false;
				}
			}

			if(BrowserType == FileChooserType.File || SelectionPattern == null)
			{
				m_directories = Directory.GetDirectories(m_currentDirectory);
				m_nonMatchingDirectories = new string[0];
			}
			else
			{
				m_directories = Directory.GetDirectories(m_currentDirectory, SelectionPattern);
				var nonMatchingDirectories = new List<string>();
				foreach(string directoryPath in Directory.GetDirectories(m_currentDirectory))
				{
					if(Array.IndexOf(m_directories, directoryPath) < 0)
					{
						nonMatchingDirectories.Add(directoryPath);
					}
				}
				m_nonMatchingDirectories = nonMatchingDirectories.ToArray();
				for(int i = 0; i < m_nonMatchingDirectories.Length; ++i)
				{
					int lastSeparator = m_nonMatchingDirectories[i].LastIndexOf(Path.DirectorySeparatorChar);
					m_nonMatchingDirectories[i] = m_nonMatchingDirectories[i].Substring(lastSeparator + 1);
				}
				Array.Sort(m_nonMatchingDirectories);
			}

			for(int i = 0; i < m_directories.Length; ++i)
			{
				m_directories[i] = m_directories[i].Substring(m_directories[i].LastIndexOf(Path.DirectorySeparatorChar) + 1);
			}

			if(BrowserType == FileChooserType.Directory || SelectionPattern == null)
			{
				m_files = Directory.GetFiles(m_currentDirectory);
				m_nonMatchingFiles = new string[0];
			}
			else
			{
				m_files = Directory.GetFiles(m_currentDirectory, SelectionPattern);
				var nonMatchingFiles = new List<string>();
				foreach(string filePath in Directory.GetFiles(m_currentDirectory))
				{
					if(Array.IndexOf(m_files, filePath) < 0)
					{
						nonMatchingFiles.Add(filePath);
					}
				}
				m_nonMatchingFiles = nonMatchingFiles.ToArray();
				for(int i = 0; i < m_nonMatchingFiles.Length; ++i)
				{
					m_nonMatchingFiles[i] = Path.GetFileName(m_nonMatchingFiles[i]);
				}
				Array.Sort(m_nonMatchingFiles);
			}
			for(int i = 0; i < m_files.Length; ++i)
			{
				m_files[i] = Path.GetFileName(m_files[i]);
			}
			Array.Sort(m_files);
			BuildContent();
			m_newDirectory = null;
		}

		protected void BuildContent()
		{
			m_directoriesWithImages = new GUIContent[m_directories.Length];
			for(int i = 0; i < m_directoriesWithImages.Length; ++i)
			{
				m_directoriesWithImages[i] = new GUIContent(m_directories[i], DirectoryImage);
			}
			m_nonMatchingDirectoriesWithImages = new GUIContent[m_nonMatchingDirectories.Length];
			for(int i = 0; i < m_nonMatchingDirectoriesWithImages.Length; ++i)
			{
				m_nonMatchingDirectoriesWithImages[i] = new GUIContent(m_nonMatchingDirectories[i], DirectoryImage);
			}
			m_filesWithImages = new GUIContent[m_files.Length];
			for(int i = 0; i < m_filesWithImages.Length; ++i)
			{
				m_filesWithImages[i] = new GUIContent(m_files[i], FileImage);
			}
			m_nonMatchingFilesWithImages = new GUIContent[m_nonMatchingFiles.Length];
			for(int i = 0; i < m_nonMatchingFilesWithImages.Length; ++i)
			{
				m_nonMatchingFilesWithImages[i] = new GUIContent(m_nonMatchingFiles[i], FileImage);
			}
		}

		public void OnGUI()
		{
			if(m_guiSkin != null)
				GUI.skin = m_guiSkin;
			
			GUILayout.BeginArea(
				new Rect(0, 0, Screen.width, Screen.height),
				m_name,
				GUI.skin.window
			);
			GUILayout.BeginHorizontal();
			for(int parentIndex = 0; parentIndex < m_currentDirectoryParts.Length; ++parentIndex)
			{
				if(parentIndex == m_currentDirectoryParts.Length - 1)
				{
					GUILayout.Label(m_currentDirectoryParts[parentIndex], CentredText);
				}
				else if(GUILayout.Button(m_currentDirectoryParts[parentIndex]))
				{
					string parentDirectoryName = m_currentDirectory;
					for(int i = m_currentDirectoryParts.Length - 1; i > parentIndex; --i)
					{
						parentDirectoryName = Path.GetDirectoryName(parentDirectoryName);
					}
					SetNewDirectory(parentDirectoryName);
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			m_scrollPosition = GUILayout.BeginScrollView(
				m_scrollPosition,
				false,
				true,
				GUI.skin.horizontalScrollbar,
				GUI.skin.verticalScrollbar,
				GUI.skin.box
			);
			m_selectedDirectory = GUILayoutx.SelectionList(
				m_selectedDirectory,
				m_directoriesWithImages,
				DirectoryDoubleClickCallback
			);
			if(m_selectedDirectory > -1)
			{
				m_selectedFile = m_selectedNonMatchingDirectory = -1;
			}
			m_selectedNonMatchingDirectory = GUILayoutx.SelectionList(
				m_selectedNonMatchingDirectory,
				m_nonMatchingDirectoriesWithImages,
				NonMatchingDirectoryDoubleClickCallback
			);
			if(m_selectedNonMatchingDirectory > -1)
			{
				m_selectedDirectory = m_selectedFile = -1;
			}
			GUI.enabled = BrowserType == FileChooserType.File;
			m_selectedFile = GUILayoutx.SelectionList(
				m_selectedFile,
				m_filesWithImages,
				FileDoubleClickCallback
			);
			GUI.enabled = true;
			if(m_selectedFile > -1)
			{
				m_selectedDirectory = m_selectedNonMatchingDirectory = -1;
			}
			GUI.enabled = false;
			GUILayoutx.SelectionList(
				-1,
				m_nonMatchingFilesWithImages
			);
			GUI.enabled = true;
			GUILayout.EndScrollView();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Cancel", GUILayout.Width(50)))
			{
				m_callback(null);
			}
			if(BrowserType == FileChooserType.File)
			{
				GUI.enabled = m_selectedFile > -1;
			}
			else
			{
				if(SelectionPattern == null)
				{
					GUI.enabled = m_selectedDirectory > -1;
				}
				else
				{
					GUI.enabled =	m_selectedDirectory > -1 ||
					(
					    m_currentDirectoryMatches &&
					    m_selectedNonMatchingDirectory == -1 &&
					    m_selectedFile == -1
					);
				}
			}
			if(GUILayout.Button("Select", GUILayout.Width(50)))
			{
				if(BrowserType == FileChooserType.File)
				{
					m_callback(Path.Combine(m_currentDirectory, m_files[m_selectedFile]));
				}
				else
				{
					if(m_selectedDirectory > -1)
					{
						m_callback(Path.Combine(m_currentDirectory, m_directories[m_selectedDirectory]));
					}
					else
					{
						m_callback(m_currentDirectory);
					}
				}
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			if(Event.current.type == EventType.Repaint)
			{
				SwitchDirectoryNow();
			}
		}

		protected void FileDoubleClickCallback(int i)
		{
			if(BrowserType == FileChooserType.File)
			{
				m_callback(Path.Combine(m_currentDirectory, m_files[i]));
			}
		}

		protected void DirectoryDoubleClickCallback(int i)
		{
			SetNewDirectory(Path.Combine(m_currentDirectory, m_directories[i]));
		}

		protected void NonMatchingDirectoryDoubleClickCallback(int i)
		{
			SetNewDirectory(Path.Combine(m_currentDirectory, m_nonMatchingDirectories[i]));
		}

	}

	public class GUILayoutx
	{

		public delegate void DoubleClickCallback(int index);

		public static int SelectionList(int selected, GUIContent[] list)
		{
			return SelectionList(selected, list, "List Item", null);
		}

		public static int SelectionList(int selected, GUIContent[] list, GUIStyle elementStyle)
		{
			return SelectionList(selected, list, elementStyle, null);
		}

		public static int SelectionList(int selected, GUIContent[] list, DoubleClickCallback callback)
		{
			return SelectionList(selected, list, "List Item", callback);
		}

		public static int SelectionList(int selected, GUIContent[] list, GUIStyle elementStyle, DoubleClickCallback callback)
		{
			for(int i = 0; i < list.Length; ++i)
			{
				Rect elementRect = GUILayoutUtility.GetRect(list[i], elementStyle);
				bool hover = elementRect.Contains(Event.current.mousePosition);

				if (hover && Event.current.type == EventType.MouseDown && Event.current.clickCount == 1) // added " && Event.current.clickCount == 1"
				{
					selected = i;
					Event.current.Use();
				}
				else if (hover && callback != null && Event.current.type == EventType.MouseDown && Event.current.clickCount == 2) //Changed from MouseUp to MouseDown
				{
					callback(i);
					Event.current.Use();
				}
				else if(Event.current.type == EventType.repaint)
				{
					elementStyle.Draw(elementRect, list[i], hover, false, i == selected, false);
				}
			}
			return selected;
		}

		public static int SelectionList(int selected, string[] list)
		{
			return SelectionList(selected, list, "List Item", null);
		}

		public static int SelectionList(int selected, string[] list, GUIStyle elementStyle)
		{
			return SelectionList(selected, list, elementStyle, null);
		}

		public static int SelectionList(int selected, string[] list, DoubleClickCallback callback)
		{
			return SelectionList(selected, list, "List Item", callback);
		}

		public static int SelectionList(int selected, string[] list, GUIStyle elementStyle, DoubleClickCallback callback)
		{
			for(int i = 0; i < list.Length; ++i)
			{
				Rect elementRect = GUILayoutUtility.GetRect(new GUIContent(list[i]), elementStyle);
				bool hover = elementRect.Contains(Event.current.mousePosition);
				if (hover && Event.current.type == EventType.MouseDown && Event.current.clickCount == 1) // added " && Event.current.clickCount == 1"
				{
					selected = i;
					Event.current.Use();
				}
				else if (hover && callback != null && Event.current.type == EventType.MouseDown && Event.current.clickCount == 2) //Changed from MouseUp to MouseDown
				{
					callback(i);
					Event.current.Use();
				}
				else if(Event.current.type == EventType.repaint)
				{
					elementStyle.Draw(elementRect, list[i], hover, false, i == selected, false);
				}
			}
			return selected;
		}

	}
}