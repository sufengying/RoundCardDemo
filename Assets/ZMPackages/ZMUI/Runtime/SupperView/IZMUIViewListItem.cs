

namespace ZM.UI
{
    public interface IZMUIViewListItem
    {
        /// <summary>
        /// 初始化列表Item
        /// </summary>
        public void InitListItem();

        /// <summary>
        /// 设置Item显示数据
        /// </summary>
        /// <param name="index">数据索引</param>
        /// <param name="data">Item数据(Class是引用类型，不存在装箱和拆箱操作，放心使用)</param>
        public void SetListItemShowData(int index,params object[] data);

        /// <summary>
        /// 脚本资源释放接口
        /// </summary>
        public void OnRelease();

    }
}
