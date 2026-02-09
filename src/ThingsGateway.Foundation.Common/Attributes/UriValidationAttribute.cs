//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
namespace ThingsGateway;

/// <summary>
/// Uri格式校验
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class UriValidationAttribute : ValidationAttribute
{
    private static readonly Regex Ipv4Regex = new Regex(@"^\d{1,3}(\.\d{1,3}){3}(:\d+)?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex Ipv6Regex = new Regex(@"^\[*::\*\](?::\d+)?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex DomainRegex = new Regex(@"^([\w+.-]+)://([\w.-]+)(:\d+)?$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    /// <inheritdoc/>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var uriString = value?.ToString();
        if (string.IsNullOrWhiteSpace(uriString)) return ValidationResult.Success;

        // 验证端口号
        if (int.TryParse(uriString, out int port))
        {
            if (port <= 0 || port > 65535)
            {
                return new ValidationResult("The port number must be an integer between 1 and 65535");
            }
        }
        else if (Ipv4Regex.IsMatch(uriString))
        {
            // IPv4 验证
            var ipPart = uriString!.Split(':')[0];
            int start = 0;
            int dotCount = 0;
            // 手动解析 IPv4 段，避免多次 Split/Parse 分配和异常开销
            for (int i = 0; i <= ipPart.Length; i++)
            {
                if (i == ipPart.Length || ipPart[i] == '.')
                {
                    var len = i - start;
                    if (len <= 0 || len > 3)
                    {
                        return new ValidationResult("Each segment of the IPv4 address value must be between 0 and 255");
                    }
                    int valueSeg = 0;
                    for (int j = start; j < i; j++)
                    {
                        char c = ipPart[j];
                        if (c < '0' || c > '9')
                        {
                            return new ValidationResult("Each segment of the IPv4 address value must be between 0 and 255");
                        }
                        valueSeg = valueSeg * 10 + (c - '0');
                        if (valueSeg > 255)
                        {
                            return new ValidationResult("Each segment of the IPv4 address value must be between 0 and 255");
                        }
                    }
                    dotCount++;
                    start = i + 1;
                }
            }
            if (dotCount != 4)
            {
                return new ValidationResult("Each segment of the IPv4 address value must be between 0 and 255");
            }
        }
        else if (!Ipv6Regex.IsMatch(uriString) && !DomainRegex.IsMatch(uriString))
        {
            // 其他格式验证失败
            return new ValidationResult("The format of the input URI string does not meet the requirements");
        }

        // 验证通过
        return ValidationResult.Success;
    }
}
