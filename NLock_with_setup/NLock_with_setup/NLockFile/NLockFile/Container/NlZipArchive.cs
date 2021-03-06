﻿using log4net;
using NLock.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace NLock.NLockFile.Container
{
    internal class NlZipArchive : IContainerInterface
    {
        #region Private variables

        private static readonly ILog Logger = LogManager.GetLogger(typeof(NlZipArchive));
        private Stream _memStream;
        private readonly object _lockThis = new object();
        private List<NlFile> _fileList;

        #endregion Private variables

        #region Public constructor

        public NlZipArchive()
        {
            InitializeStream();
        }

        #endregion Public constructor

        #region Methods

        #region Public

        public void Dispose()
        {
            if (_memStream != null)
            {
                _memStream.Dispose();
            }
            _memStream = null;
            if (_fileList != null)
            {
                _fileList.Clear();
            }
            _fileList = null;
        }

        public bool AddFile(string filePath)
        {
            lock (_lockThis)
            {
                Logger.Debug(filePath);
                if (!IsDirectory(filePath))
                {
                    Logger.Debug("Is a file");
                    using (FileStream fs = File.OpenRead(filePath))
                    using (ZipArchive zip = new ZipArchive(_memStream, ZipArchiveMode.Update, true))
                    {
                        Logger.Debug("Reading file : " + filePath + " Length : " + fs.Length);
                        var compressionLevel = (CompressionLevel) Settings.Default.compressionLevel;
                        var entry = zip.CreateEntry(filePath, compressionLevel);

                        using (var entryStream = entry.Open())
                        {
                            fs.CopyTo(entryStream, 50);
                        }
                        Logger.Debug("Entry created  : " + filePath);
                    }
                    return true;
                }
                else
                {
                    Logger.Debug("Is a directory");
                    return AddFileWithingAFolder(filePath);
                }
            }
        }

        public bool AddFolder(string folderPath, bool recursively = false, string folderName = null)
        {
            try
            {
                if (!recursively)
                {
                    Logger.Debug("Adding folder non recursively folderPath : " + folderPath);
                    var fileList = Directory.GetFiles(folderPath + "\\");

                    Logger.Debug("Folder path : " + folderPath);
                    if (fileList.Length <= 0)
                    {
                        Logger.Warn("Empty directory : " + folderPath);
                        return false;
                    }

                    foreach (string file in fileList)
                    {
                        var listi = folderPath.Split(Path.DirectorySeparatorChar);
                        AddFile("-D <dir>" + listi[listi.Length - 1] + "<dir> " + file);
                    }
                }
                else
                {
                    Logger.Debug("Adding folder recursively folderPath : " + folderPath);

                    var fileList = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
                    var listi = folderPath.Split(Path.DirectorySeparatorChar);

                    if (fileList.Length <= 0)
                    {
                        Logger.Warn("Empty directory : " + folderPath);
                    }
                    else
                    {
                        foreach (string file in fileList)
                        {
                            if (folderName == null)
                            {
                                AddFile("-D <dir>" + listi[listi.Length - 1] + "<dir> " + file);
                            }
                            else
                            {
                                AddFile("-D <dir>" + folderName + "<dir> " + file);
                            }
                        }

                        // Recursive calling on each sub directory in next level
                    }
                    var dirList = Directory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);
                    if (dirList.Length <= 0)
                    {
                        return false;
                    }
                    else
                    {
                        foreach (string dir in dirList)
                        {
                            var listd = dir.Split(Path.DirectorySeparatorChar);
                            AddFolder(dir, true, folderName + "\\" + listd[listd.Length - 1]);
                            Logger.Debug(folderName + "\\" + listd[listd.Length - 1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("AddFolder " + ex);
                throw;
            }

            return false;
        }

        public List<NlFile> GetNlFileList()
        {
            var t = Task.Run(() =>
            {
                GetNlFileListT();
            });
            t.Wait();
            return _fileList;
        }

        public bool RemoveFile(string filePath)
        {
            using (ZipArchive zip = new ZipArchive(_memStream, ZipArchiveMode.Update, true))
            {
                var entries = zip.Entries;
                for (int x = entries.Count - 1; x >= 0; x--)
                {
                    var e = entries[x];
                    if (e.Name == filePath.Trim())
                    {
                        entries[x].Delete();

                        return true;
                    }
                }
            }
            return false;
        }

        public byte[] GetContent()
        {
            var curPosition = _memStream.Position;
            _memStream.Position = 0;
            var outPutBuffer = ((MemoryStream) _memStream).ToArray();
            _memStream.Position = curPosition;
            return outPutBuffer;
        }

        public bool LoadFromMemory(byte[] content)
        {
            InitializeStream();
            _memStream.Write(content, 0, content.Length);
            return true;
        }

        public bool ExtractToFolder(string folderPath)
        {
            Logger.Debug("Extracting to : " + folderPath);
            try
            {
                return ExtractTo(folderPath);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(folderPath);
                return ExtractTo(folderPath);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.ToString());
                throw;
            }
        }

        private static bool IsDirectory(string fullPath)
        {
            var list = fullPath.Split();
            if (list.Length < 3)
                return false;

            if (list[0] == "-D")
            {
                return true;
            }
            return false;
        }

        #endregion Public

        #region Private

        private bool ExtractTo(string folderPath)
        {
            try
            {
                _memStream.Seek(0, SeekOrigin.Begin);
                using (ZipArchive zip = new ZipArchive(_memStream, ZipArchiveMode.Read, true))
                {
                    var entrie = zip.Entries;
                    foreach (var entry in entrie)
                    {
                        if (!IsDirectory(entry.FullName))
                        {
                            var extractionFullPath = folderPath + entry.Name;
                            if (File.Exists(extractionFullPath))
                            {
                                extractionFullPath = GetPossibleFileName(folderPath, entry.Name);
                            }
                            using (var fileStream = new FileStream(extractionFullPath, FileMode.Create))
                            {
                                var openedEntry = entry.Open();
                                openedEntry.CopyTo(fileStream);
                                fileStream.Close();
                            }
                        }
                        else
                        {
                            Logger.Debug("entry.FullName " + entry.FullName);

                            var listi = entry.FullName.Split(new[] { "#####" }, StringSplitOptions.RemoveEmptyEntries);

                            var rootPath = new StringBuilder();
                            rootPath.Append(folderPath);
                            rootPath.Append(listi[1]);

                            if (!Directory.Exists(rootPath.ToString()))
                            {
                                Directory.CreateDirectory(rootPath + "\\");
                            }
                            rootPath.Append("\\");

                            var extractionFullPath = rootPath + entry.Name;
                            Logger.Debug("Extracting as a directory item " + rootPath);

                            if (File.Exists(extractionFullPath))
                            {
                                extractionFullPath = GetPossibleFileName(rootPath.ToString(), entry.Name);
                            }
                            using (var fileStream = new FileStream(extractionFullPath, FileMode.Create))
                            {
                                var openedEntry = entry.Open();
                                openedEntry.CopyTo(fileStream);
                                fileStream.Close();
                            }
                        }
                    }
                    return true;
                    //TODO : next version file by file fail report
                }
            }
            catch (Exception)
            {
                return false;
                //TODO : Check and handle the errors
            }
        }

        private static string GetPossibleFileName(string folderpath, string fileName)
        {
            var finalName = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);

            var i = 1;
            var fullPath = folderpath + finalName + "_" + i + ext;
            while (File.Exists(fullPath))
            {
                i++;
                if (i > 200)
                {
                    var randi = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
                    fullPath = folderpath + finalName + "_" + "Copy_" + randi + ext;
                }
                else
                {
                    fullPath = folderpath + finalName + "_" + i + ext;
                }
            }
            return fullPath;
        }

        private void InitializeStream()
        {
            _memStream = new MemoryStream();
        }

        private bool AddFileWithingAFolder(string filePath)
        {
            lock (_lockThis)
            {
                try
                {
                    Logger.Debug(filePath);
                    var listi = filePath.Split(new[] { "<dir>" }, StringSplitOptions.RemoveEmptyEntries);

                    using (FileStream fs = File.OpenRead(listi[2]))
                    using (ZipArchive zip = new ZipArchive(_memStream, ZipArchiveMode.Update, true))
                    {
                        Logger.Debug("Reading file : " + listi[2] + " Length : " + fs.Length);
                        var entry = zip.CreateEntry(listi[0] + "#####" + listi[1] + "#####" + listi[2], CompressionLevel.Optimal);
                        Logger.Debug("Full name : " + entry.FullName + " Name : " + entry.Name + " GetType: " + entry.GetType());
                        using (var entryStream = entry.Open())
                        {
                            fs.CopyTo(entryStream, 1024);
                        }
                        Logger.Debug("Entry created  : " + listi[2] + "CompressionLevel.Optimal");
                    }
                    return true;
                }
                catch (Exception)
                {
                    // Throw on empty folders within
                    throw;
                }
            }
        }

        private void GetNlFileListT()
        {
            lock (_lockThis)
            {
                Logger.Debug("");
                try
                {
                    _fileList = new List<NlFile>();
                    using (ZipArchive zip = new ZipArchive(_memStream, ZipArchiveMode.Read, true))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            Logger.Debug("Entry : " + entry.FullName + " size : " + entry.Length + " compressed size : " + entry.CompressedLength);
                            var listi = entry.FullName.Split(new[] { "#####" }, StringSplitOptions.RemoveEmptyEntries);

                            if (listi.Length > 2)
                            {
                                var fullPath = new StringBuilder();
                                for (int i = 2; i < listi.Length; i++)
                                {
                                    fullPath.Append(listi[i] + " ");
                                }

                                var nlfile = new NlFile(fullPath.ToString())
                                {
                                    OriginalSize = entry.Length,
                                    CompressedSize = entry.CompressedLength,
                                    FilePath = listi[1]
                                };

                                _fileList.Add(nlfile);
                            }
                            else
                            {
                                var nlfile = new NlFile(entry.FullName)
                                {
                                    OriginalSize = entry.Length,
                                    CompressedSize = entry.CompressedLength
                                };
                                _fileList.Add(nlfile);
                            }
                        }
                    }
                    Logger.Debug("Sorting list");
                    _fileList.Sort((a, b) => string.Compare(a.FileName, b.FileName, StringComparison.Ordinal));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                    _fileList = null;
                    //throw new NLockFileCorruptedException("Get File List failed");
                }
                //TODO : check for reading before adding any item, Central Directory corrupt error
            }
        }

        #endregion Private

        #endregion Methods
    }
}