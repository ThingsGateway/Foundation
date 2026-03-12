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

public class RequestPool<TRequest> : Pool<TRequest>, IRequestPool where TRequest : DeviceMessage, new()
{
    /// <summary>实例化字符串池。GC2时回收</summary>
    public RequestPool() : base(0, true) { Max = 1024; }

    /// <summary>创建</summary>
    /// <returns></returns>
    protected override TRequest OnCreate() => new TRequest() { RequestPool = this, OperCode = -1, Sign = -1 };

    /// <summary>归还</summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override Boolean Return(TRequest value)
    {
        value.Sign = -1;
        value.OperCode = -1;
        return base.Return(value);
    }

    public Boolean Put(DeviceMessage value)
    {
        if (value is TRequest request)
        {
            return Return(request);
        }
        return false;
    }
}

public interface IRequestPool
{
    public Boolean Put(DeviceMessage value);
}