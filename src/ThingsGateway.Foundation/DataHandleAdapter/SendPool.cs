//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Common.Collections;

namespace ThingsGateway.Foundation;

public class SendPool<TRequest> : Pool<TRequest>, ISendPool where TRequest : SendMessage, new()
{
    /// <summary>实例化字符串池。GC2时回收</summary>
    public SendPool() : base(0, true) { Max = 256; }

    /// <summary>创建</summary>
    /// <returns></returns>
    protected override TRequest OnCreate() => new TRequest() { SendPool = this, Sign = -1 };

    public override TRequest Get()
    {
        var data = base.Get();
        data.Reset();
        return data;
    }

    /// <summary>归还</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override Boolean Return(TRequest value)
    {
        value.Sign = -1;
        return base.Return(value);
    }
    public Boolean Put(SendMessage value)
    {
        if (value is TRequest request)
        {
            return Return(request);
        }
        return false;
    }

}
public interface ISendPool
{
    public Boolean Put(SendMessage value);
}