namespace ThingsGateway.Foundation.Common;

/// <summary>
/// 控制台行为
/// </summary>
public sealed class ConsoleAction
{
    private readonly Dictionary<string, ConsoleActionInfo> m_actions = new Dictionary<string, ConsoleActionInfo>();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="helpOrder">帮助信息指令，如："h|help|?"</param>
    public ConsoleAction(string helpOrder = "h|help|?")
    {
        this.HelpOrder = helpOrder;

        this.Add(helpOrder, "帮助信息", this.ShowAll);
    }

    /// <summary>
    /// 执行异常
    /// </summary>
    public event Action<Exception>? OnException;

    /// <summary>
    /// 帮助信息指令
    /// </summary>
    public string HelpOrder { get; }

    /// <summary>
    /// 所有命令信息
    /// </summary>
    public IReadOnlyList<ConsoleActionInfo> AllActionInfos => this.m_actions.Values.Where(a => a.FullOrder != this.HelpOrder).ToList();

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="order">指令，多个指令用"|"分割</param>
    /// <param name="description">描述</param>
    /// <param name="action"></param>
    public void Add(string order, string description, Action action)
    {
        Task Run()
        {
            action.Invoke();
            return Task.CompletedTask;
        }
        this.Add(order, description, Run);
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="order">指令，多个指令用"|"分割</param>
    /// <param name="description">描述</param>
    /// <param name="action"></param>
    public void Add(string order, string description, Func<Task> action)
    {
        var orders = order.ToLower().Split('|');
        foreach (var item in orders)
        {
            this.m_actions.Add(item, new ConsoleActionInfo(description, order, action));
        }
    }

    /// <summary>
    /// 执行，返回值仅表示是否有这个指令，异常获取请使用<see cref="OnException"/>
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public async Task<bool> RunAsync(string order)
    {
        if (this.m_actions.TryGetValue(order.ToLower(), out var vAction))
        {
            try
            {
                await vAction.Action.Invoke().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                WriteError($"执行命令时发生错误: {ex.GetStackTrace()}");
                OnException?.Invoke(ex);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 运行
    /// </summary>
    public async Task RunCommandLineAsync()
    {
        while (true)
        {
            WritePrompt("请输入命令: ");
            var str = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(str))
            {
                continue;
            }

            if (!await this.RunAsync(str).ConfigureAwait(true))
            {
                WriteWarning($"没有找到命令 '{str}'，输入 '{this.HelpOrder.Split('|')[0]}' 查看帮助信息。");
            }
        }
    }

    /// <summary>
    /// 显示所有注册指令
    /// </summary>
    public void ShowAll()
    {
        WriteTitle("\n可用命令列表:");
        WriteLine();

        var maxOrderLength = this.m_actions.Values.Max(a => a.FullOrder.Length);
        var separator = new string('─', maxOrderLength + 4);

        var distinctActions = new List<string>();
        foreach (var item in this.m_actions.OrderBy(a => a.Value.FullOrder))
        {
            if (!distinctActions.Contains(item.Value.FullOrder.ToLower()))
            {
                distinctActions.Add(item.Value.FullOrder.ToLower());

                WriteCommand($"  [{item.Value.FullOrder}]");
                var padding = maxOrderLength - item.Value.FullOrder.Length + 2;
                Console.Write(new string(' ', padding));
                WriteDescription(item.Value.Description);
                WriteLine();
            }
        }

        WriteLine();
    }

    /// <summary>
    /// 写入标题
    /// </summary>
    private static void WriteTitle(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入命令
    /// </summary>
    private static void WriteCommand(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入描述
    /// </summary>
    private static void WriteDescription(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入提示
    /// </summary>
    private static void WritePrompt(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入警告
    /// </summary>
    private static void WriteWarning(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入错误
    /// </summary>
    private static void WriteError(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入信息
    /// </summary>
    private static void WriteInfo(string text)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(text);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// 写入空行
    /// </summary>
    private static void WriteLine()
    {
        Console.WriteLine();
    }
}