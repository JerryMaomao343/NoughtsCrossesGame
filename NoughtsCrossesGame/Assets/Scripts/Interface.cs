public interface IUIHandler
{
    /// <summary>
    /// 用传入的参数初始化UI。参数可以通过装箱传递。
    /// </summary>
    /// <param name="param">初始化参数</param>
    void Init(object param);
}

