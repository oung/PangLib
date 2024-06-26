﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PangLib.DAT;

/// <summary>
/// Main DAT file class
/// </summary>
public class DATFile
{
    public List<string> Entries = new List<string>();

    private Encoding FileEncoding;

    /// <summary>
    /// Parses the data from the DAT file
    /// </summary>
    /// <param name="data">Stream containing DAT file data</param>
    private void Parse(Stream data)
    {
        using (BinaryReader reader = new BinaryReader(data, FileEncoding))
        {
            List<char> stringChars = new List<char>();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                if (reader.PeekChar() != 0x00)
                {
                    stringChars.Add(reader.ReadChar());
                }
                else
                {
                    char[] chars = stringChars.ToArray();
                    byte[] bytes = FileEncoding.GetBytes(chars);

                    Entries.Add(FileEncoding.GetString(bytes));

                    reader.BaseStream.Seek(1L, SeekOrigin.Current);
                    stringChars = new List<char>();
                }
            }
        }
    }

    /// <summary>
    /// Tries to get the file encoding based on
    /// the naming scheme of the files
    ///
    /// Falls back on UTF-8 as default
    /// </summary>
    /// <param name="filePath">File name of the DAT file</param>
    /// <returns>Encoding of the DAT file</returns>
    private Encoding GetEncodingFromFileName(string filePath)
    {
        if (filePath == null)
        {
            throw new InvalidOperationException("No file path given to get encoding from, use SetEncoding() method!");
        }

        string fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        Encoding targetEncoding;

        switch (fileName)
        {
            case "korea":
                targetEncoding = Encoding.GetEncoding(51949);
                break;
            case "japan":
                targetEncoding = Encoding.GetEncoding(932);
                break;
            case "english":
                targetEncoding = Encoding.ASCII;
                break;
            case "thailand":
                targetEncoding = Encoding.GetEncoding(874);
                break;
            case "indonesia":
                targetEncoding = Encoding.GetEncoding(65001);
                break;
            case "brasil":
            case "spanish":
            case "german":
            case "french":
                targetEncoding = Encoding.GetEncoding(1252);
                break;
            default:
                targetEncoding = Encoding.GetEncoding(65001);
                break;
        }

        return targetEncoding;
    }

    /// <summary>
    /// Sets the encoding to be used by the DATFile instance
    /// </summary>
    /// <param name="encoding">Encoding to set</param>
    public void SetEncoding(Encoding encoding)
    {
        FileEncoding = encoding;
    }
        
    /// <summary>
    /// Returns the encoding used by the DATFile instance
    /// </summary>
    public Encoding GetEncoding()
    {
        return FileEncoding;
    }

    /// <summary>
    /// Load a DAT file into a DATFile instance
    /// </summary>
    /// <param name="filePath">File path to load the DAT file from</param>
    public static DATFile Load(string filePath)
    {
        DATFile DAT = new DATFile();

        DAT.SetEncoding(DAT.GetEncodingFromFileName(filePath));
        DAT.Parse(File.Open(filePath, FileMode.Open));

        return DAT;
    }

    /// <summary>
    /// Save a DATFile instance to a file
    /// </summary>
    /// <param name="filePath">File path to save the DAT file to</param>
    public void Save(string filePath)
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write), FileEncoding))
        {
            foreach (string entry in Entries)
            {
                writer.Write(entry.ToCharArray());
                writer.Write((byte)0);
            }
        }
    }
}