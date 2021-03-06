
using DataLoadLib.Global;
using System.Collections.Generic;
using DataFileEnum;

namespace DataFileEnum
{
	public class CGoodsData : CDataFileBase
	{
		public EGoodsType GoodsType { get; private set; }
		public string Bundle { get; private set; }
		public string SmallIcon { get; private set; }
		public string LargeIcon { get; private set; }
		public bool IsUseShopShortCut { get; private set; }
		public bool IsUseMaxValue { get; private set; }

		public CGoodsData(TableInfo cInfo) : base(cInfo) { }

		protected override void SetInfo(TableInfo cInfo)
		{
			this.GoodsType = (EGoodsType)cInfo.GetIntValue(1);
			this.Bundle = cInfo.GetStrValue(2);
			this.SmallIcon = cInfo.GetStrValue(3);
			this.LargeIcon = cInfo.GetStrValue(4);
			this.IsUseShopShortCut = cInfo.GetBoolValue(5);
			this.IsUseMaxValue = cInfo.GetBoolValue(6);
		}
	}
}
