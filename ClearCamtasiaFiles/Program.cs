internal class Program
{
    const string deletedTrecFilesFolder = "DeletedTrecFilesFolder";
    const string nonProjectTrecFilesFolder = "NonProjectTrecFiles";
    const string deletedMp4filesFolder = "DeletedMp4Files";
    const string unusedMp4filesFolder = "UnusedMp4Files";

    private static void Main(string[] args)
    {
        string curDir = Directory.GetCurrentDirectory();

        List<string> filesTrec = Directory.GetFiles(curDir)
            .Where(i => i.EndsWith(".trec"))
            .ToList();
        Console.WriteLine($".TREC files found: \t{filesTrec.Count}");

        List<string> filesMp4 = Directory.GetFiles(curDir)
            .Where(i => i.EndsWith(".mp4"))
            .ToList();
        Console.WriteLine($".MP4 files found: \t{filesMp4.Count}");

        List<string> folders = Directory.GetDirectories(curDir)
            .Where(i => i.EndsWith(".tscproj"))
            .ToList();
        Console.WriteLine($".TSCPROJ folders found: \t{folders.Count}");

        Dictionary<string, List<string>> foldersAndFiles = new Dictionary<string, List<string>>();
        foreach(string folder in folders)
        {
            List<string> curDirFiles = Directory.GetFiles(folder)
                .Select(i => Path.GetFileName(i))
                .ToList();
            foldersAndFiles.Add(folder, curDirFiles);
        }

        

        Console.WriteLine();
        Console.WriteLine("Clear TREC files...");

        foreach (string curTrecFile in filesTrec)
        {
            Console.WriteLine($"Current TREC to check:\t {curTrecFile}");
            string fileName = Path.GetFileName(curTrecFile);
            long fileLength = new FileInfo(fileName).Length;
            bool fileIsDone = false;

            foreach(var kvp in foldersAndFiles)
            {
                if(kvp.Value.Contains(fileName))
                {
                    Console.WriteLine($"File is found in folder: {kvp.Key}");
                    string projTrecFilename = Path.Combine(kvp.Key, fileName);
                    long projTrecFileLength = new FileInfo(projTrecFilename).Length;

                    if(projTrecFileLength == fileLength)
                    {
                        Console.WriteLine($"The files are identical, file length: {fileLength}");
                        string movedFile = MoveFileToFolderSafe(curTrecFile, deletedTrecFilesFolder);
                        Console.WriteLine($"File moved to: {movedFile}");
                        fileIsDone = true;
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"The files have the same name, but different size, no action");
                        continue;
                    }
                    
                }
            }
            if (fileIsDone) continue;

            
            string movedFileToUnused = MoveFileToFolderSafe(curTrecFile, nonProjectTrecFilesFolder);
            Console.WriteLine($"File is not used in projects. Moved to: {movedFileToUnused}");

        }



        Console.WriteLine();
        Console.WriteLine("Clear MP4 files...");

        foreach(string curMp4File in filesMp4)
        {
            Console.WriteLine($"Cur MP4 file: {curMp4File}");
            string filename = Path.GetFileName(curMp4File);
            string filenameNoExt = Path.GetFileNameWithoutExtension(curMp4File);
            long fileLength = new FileInfo(curMp4File).Length;


            bool fileIsDone = false;
            foreach(var kvp in foldersAndFiles)
            {
                string folder = kvp.Key;
                string folderNameShort = new DirectoryInfo(folder).Name;
                folderNameShort = folderNameShort.Replace(".tscproj", "");
                if(folderNameShort == filenameNoExt)
                {
                    Console.WriteLine($"Found project folder: {folder}");
                    string projFilePath = Path.Combine(folder, filename);
                    long projFileLength = 0;
                    if (kvp.Value.Contains(filename))
                        projFileLength = new FileInfo(projFilePath).Length;

                    string folderToMove = folder;
                    if (fileLength == projFileLength)
                    {
                        Console.WriteLine("identical MP4 file is found in project folder and will be deleted.");
                        folderToMove = deletedMp4filesFolder;
                    }

                    string movedMp4 = MoveFileToFolderSafe(curMp4File, folderToMove);
                    Console.WriteLine($"File is moved to: {movedMp4}");
                    fileIsDone = true;
                }
            }
            if(!fileIsDone)
            {
                string movedMp4 = MoveFileToFolderSafe(curMp4File, unusedMp4filesFolder);
                Console.WriteLine($"Mp4 file is unused. Moved to: {movedMp4}");
            }
        }

        Console.WriteLine("Done! Press any key to exit...");
        Console.ReadKey();
    }

    private static string MoveFileToFolderSafe(string oldFilename, string newFolderShortName)
    {
        string newFolder = Path.Combine(Directory.GetCurrentDirectory(), newFolderShortName);
        if (!Directory.Exists(newFolder))
            Directory.CreateDirectory(newFolder);


        string shortFilename = Path.GetFileName(oldFilename);
        string newFilename = Path.Combine(newFolder, shortFilename);

        string newFilenameNoExt = Path.GetFileNameWithoutExtension(newFilename);
        string filenameExt = Path.GetExtension(newFilename);
        string tryNewFilename = newFilename;
        int newNameCounter = 1;
        while(File.Exists(tryNewFilename))
        {
            newNameCounter++;
            tryNewFilename = $"{newFilenameNoExt} ({newNameCounter}){filenameExt}";
        }

        try
        {
            File.Move(oldFilename, tryNewFilename);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Console.ReadKey();
            throw new Exception(ex.Message);
        }
        return tryNewFilename;
    }
}