//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

using ThingsGateway.Foundation.Common.Json.Extension;

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

/// <summary>
/// Json字符串转到对应类
/// </summary>
public class JsonToClassConverter<TState> : ISerializerFormatter<string, TState>
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public int Order { get; set; } = 100;

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public bool TryDeserialize(TState state, in string source, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType, out object target)
    {
        try
        {
            target = NewtonsoftJsonExtension.FromJsonNetString(source, targetType);
            return true;
        }
        catch
        {
            target = default;
            return false;
        }
    }

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
    public bool TrySerialize<TTarget>(TState state, in TTarget target, out string source)
    {
        try
        {
            source = NewtonsoftJsonExtension.ToJsonNetString(target, false);
            return true;
        }
        catch (Exception)
        {
            source = default;
            return false;
        }
    }


}
