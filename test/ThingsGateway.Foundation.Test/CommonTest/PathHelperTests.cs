using ThingsGateway.Foundation.Common.Extension;
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。

namespace ThingsGateway.Foundation.Common.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class PathHelperTests
    {
        private string _tempDir = Path.Combine(Path.GetTempPath(), "PathHelperTests");

        [TestInitialize]
        public void Setup()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
            Directory.CreateDirectory(_tempDir);
        }


        [TestMethod]
        public void GetRelativePath_SamePath_ReturnsDot()
        {
            var p = Path.Combine(_tempDir, "a");
            Directory.CreateDirectory(p);
            var result = PathHelper.GetRelativePath(p, p);
            Assert.AreEqual(".", result);
        }

        [TestMethod]
        public void GetRelativePath_Subdirectory_ReturnsRelative()
        {
            var baseDir = Path.Combine(_tempDir, "root");
            var subDir = Path.Combine(baseDir, "child", "file.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(subDir)!);

            var rel = PathHelper.GetRelativePath(baseDir, subDir);
            Assert.AreEqual($"child{Path.DirectorySeparatorChar}file.txt", rel);
        }

        [TestMethod]
        public void GetRelativePath_ParentDirectory_ReturnsDotDot()
        {
            var parent = Path.Combine(_tempDir, "root");
            var child = Path.Combine(parent, "a", "b", "c.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(child)!);

            var rel = PathHelper.GetRelativePath(parent, child);
            Assert.AreEqual("a\\b\\c.txt", rel); // 现在是从 root 到 child，会包含“..”路径
        }


        [TestMethod]
        public void GetRelativePath_DifferentRoot_ReturnsAbsolute()
        {
            string basePath;
            string otherPath;

            if (OperatingSystem.IsWindows())
            {
                var driveA = Path.GetPathRoot(Environment.CurrentDirectory)!;
#pragma warning disable CA1867 // 使用字符重载
                var driveB = driveA.StartsWith("C", StringComparison.OrdinalIgnoreCase)
                    ? driveA.Replace("C", "D")
                    : "D:\\";
#pragma warning restore CA1867 // 使用字符重载

                basePath = Path.Combine(driveA, "folder1");
                otherPath = Path.Combine(driveB, "folder2");
            }
            else
            {
                basePath = "/mnt/folder1";
                otherPath = "/opt/folder2";
            }

            var result = PathHelper.GetRelativePath(basePath, otherPath);

            if (OperatingSystem.IsWindows())
                Assert.IsTrue(Path.IsPathRooted(result), $"Expected absolute path, but got: {result}");
            else
                Assert.IsTrue(Path.IsPathRooted(result) || result.StartsWith(".."),
                    $"Expected absolute or parent path, got: {result}");
        }


        [TestMethod]
        public void GetRelativePath_ThrowsOnNull()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => PathHelper.GetRelativePath(null!, "a"));
            Assert.ThrowsExactly<ArgumentNullException>(() => PathHelper.GetRelativePath("a", null!));
        }

        [TestMethod]
        public void CombinePathReplace_Works()
        {
            var result = PathHelper.CombinePathReplace("folder", "sub", "file.txt");
            Assert.AreEqual("folder/sub/file.txt", result.Replace('\\', '/'));
        }

        [TestMethod]
        public void CombinePathReplace_EmptyInput_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, PathHelper.CombinePathReplace());
        }

        [TestMethod]
        public void EnsureDirectory_FilePath_CreatesParentDir()
        {
            var filePath = Path.Combine(_tempDir, "nested", "file.txt");
            var dir = filePath.EnsureDirectory(isfile: true);

            Assert.IsTrue(Directory.Exists(dir));
            Assert.EndsWith("nested", dir);
        }

        [TestMethod]
        public void EnsureDirectory_DirectoryPath_CreatesDir()
        {
            var dirPath = Path.Combine(_tempDir, "nested") + Path.DirectorySeparatorChar;
            var dir = dirPath.EnsureDirectory(isfile: false);

            Assert.IsTrue(Directory.Exists(dir));
        }

        [TestMethod]
        public void EnsureDirectory_Existing_ReturnsOriginal()
        {
            var dir = _tempDir.EnsureDirectory();
            Assert.AreEqual(_tempDir, dir);
        }

        [TestMethod]
        public void EnsureDirectory_Null_ReturnsNull()
        {
            string? result = ((String?)null).EnsureDirectory();
            Assert.IsNull(result);
        }
    }
}
