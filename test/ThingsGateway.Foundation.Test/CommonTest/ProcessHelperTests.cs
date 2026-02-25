using ThingsGateway.Foundation.Common.Extension;
using Xunit;

namespace ThingsGateway.Foundation.Common.Tests
{
    public class ProcessHelperTests
    {
        [Fact]
        public void GetProcessId_ShouldReturnCurrentProcessId()
        {
            // Arrange
            var expected = Environment.ProcessId;

            // Act
            var result = ProcessHelper.GetProcessId();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetProcessId_ShouldCacheValue()
        {
            // Arrange
            var id1 = ProcessHelper.GetProcessId();

            // Act
            var id2 = ProcessHelper.GetProcessId();

            // Assert
            Assert.Equal(id1, id2);
        }

        [Fact]
        public void Execute_EchoCommand_ReturnsExpectedOutput()
        {
            // Arrange
            var cmd = GetEchoCommand();
            var args = GetEchoArguments("HelloWorld");

            // Act
            var output = ProcessHelper.Execute(cmd, args, 2000);

            // Assert
            Assert.NotNull(output);
            Assert.Contains("HelloWorld", output);
        }

        [Fact]
        public void Execute_InvalidCommand_ReturnsNull()
        {
            // Arrange
            var result = ProcessHelper.Execute("this_command_does_not_exist", msWait: 1000);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Execute_CommandWithEncoding_Works()
        {
            // Arrange
            var cmd = GetEchoCommand();
            var args = GetEchoArguments("你好，世界！");

            // Act
            var output = ProcessHelper.Execute(cmd, args, 2000);

            // Assert
            Assert.NotNull(output);
            Assert.Contains("你好", output);
        }

        [Fact]
        public void Execute_Timeout_KillsProcess()
        {
            // Arrange
            var cmd = GetSleepCommand();

            // Act
            var output = ProcessHelper.Execute(cmd, GetSleepArgs(5), msWait: 1000);

            // Assert
            Assert.True(string.IsNullOrEmpty(output));
        }

        /// <summary>
        /// 获取跨平台 echo 命令
        /// </summary>
        private static string GetEchoCommand()
        {
            return OperatingSystem.IsWindows() ? "cmd" : "/bin/echo";
        }

        private static string GetEchoArguments(string message)
        {
            return OperatingSystem.IsWindows() ? $"/c echo {message}" : message;
        }

        /// <summary>
        /// 获取跨平台 sleep 命令
        /// </summary>
        private static string GetSleepCommand()
        {
            return OperatingSystem.IsWindows() ? "timeout" : "/bin/sleep";
        }

        private static string GetSleepArgs(int seconds)
        {
            return OperatingSystem.IsWindows() ? $"{seconds} >nul" : $"{seconds}";
        }
    }
}
