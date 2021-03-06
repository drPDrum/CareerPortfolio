
using System.Collections.Generic;
using DataFileEnum;
namespace ProjectS.Protocol
{
	/// <summary>
	/// Shelter : 500
	/// </summary>
	public class getShelterData : CProtocolBase
	{
	

		public getShelterData()
		{
			 this.cmd = 500;
		}

		public class result : CResponseDataBase
		{
			public shelter shelter;
			public int wall_remain_time;
			public device[] devices;
			public actor[] actors;
			public item[] equipments;
			public int event_flag;
		}
	}
	/// <summary>
	/// Shelter : 510
	/// </summary>
	public class doMoveToShelter : CProtocolBase
	{
	

		public doMoveToShelter()
		{
			 this.cmd = 510;
		}

		public class result : CResponseDataBase
		{
			public shelter shelter;
			public actor[] actors;
			public device[] devices;
		}
	}
	/// <summary>
	/// Shelter : 520
	/// </summary>
	public class doRepairToShelter : CProtocolBase
	{
		public int goods_type;

		public doRepairToShelter()
		{
			 this.cmd = 520;
		}

		public class result : CResponseDataBase
		{
			public goods_info[] goods_infos;
			public shelter shelter;
		}
	}
	/// <summary>
	/// Shelter : 560
	/// </summary>
	public class cancelToRepairShelter : CProtocolBase
	{
	

		public cancelToRepairShelter()
		{
			 this.cmd = 560;
		}

		public class result : CResponseDataBase
		{
			public goods_info[] goods_infos;
		}
	}
	/// <summary>
	/// Shelter : 525
	/// </summary>
	public class doImmediateRepairToShelter : CProtocolBase
	{
	

		public doImmediateRepairToShelter()
		{
			 this.cmd = 525;
		}

		public class result : CResponseDataBase
		{
			public goods_info[] goods_infos;
		}
	}
	/// <summary>
	/// Shelter : 530
	/// </summary>
	public class doExtendToShelter : CProtocolBase
	{
		public int goods_type;

		public doExtendToShelter()
		{
			 this.cmd = 530;
		}

		public class result : CResponseDataBase
		{
			public goods_info[] goods_infos;
			public shelter shelter;
		}
	}
	/// <summary>
	/// Shelter : 540
	/// </summary>
	public class completeExtendShelter : CProtocolBase
	{
	

		public completeExtendShelter()
		{
			 this.cmd = 540;
		}

		public class result : CResponseDataBase
		{
			public shelter shelter;
		}
	}
	/// <summary>
	/// Shelter : 550
	/// </summary>
	public class cancelToExtendShelter : CProtocolBase
	{
	

		public cancelToExtendShelter()
		{
			 this.cmd = 550;
		}

		public class result : CResponseDataBase
		{
			public goods_info[] goods_infos;
		}
	}
	/// <summary>
	/// Shelter : 545
	/// </summary>
	public class doImmediateExtendToShelter : CProtocolBase
	{
	

		public doImmediateExtendToShelter()
		{
			 this.cmd = 545;
		}

		public class result : CResponseDataBase
		{
			public goods_info[] goods_infos;
		}
	}
	/// <summary>
	/// Shelter : 600
	/// </summary>
	public class getDeviceData : CProtocolBase
	{
	

		public getDeviceData()
		{
			 this.cmd = 600;
		}

		public class result : CResponseDataBase
		{
			public device[] devices;
		}
	}
	/// <summary>
	/// Shelter : 610
	/// </summary>
	public class doDeviceSetup : CProtocolBase
	{
	
		public int device_id;
		public int shelter_level_id;
		public int goods_type;
		public int tutorial_id;

		public doDeviceSetup()
		{
			 this.cmd = 610;
		}

		public class result : CResponseDataBase
		{
		
			public device device;
			public goods_info[] goods_infos;
		}
	}
	/// <summary>
	/// Shelter : 620
	/// </summary>
	public class cancelToDevice : CProtocolBase
	{
	
		public long device_key;
		public int shelter_level_id;

		public cancelToDevice()
		{
			 this.cmd = 620;
		}

		public class result : CResponseDataBase
		{
		
			public device device;
			public goods_info[] goods_infos;
		}
	}
	/// <summary>
	/// Shelter : 630
	/// </summary>
	public class completeToDevice : CProtocolBase
	{
	
		public long device_key;
		public int shelter_level_id;
		public int tutorial_id;

		public completeToDevice()
		{
			 this.cmd = 630;
		}

		public class result : CResponseDataBase
		{
			public user_level_exp user_level_exp;
			public goods_info[] goods_infos;
			public device device;
		}
	}
	/// <summary>
	/// Shelter : 635
	/// </summary>
	public class doImmediateDevice : CProtocolBase
	{
	
		public long device_key;

		public doImmediateDevice()
		{
			 this.cmd = 635;
		}

		public class result : CResponseDataBase
		{
			public goods_info[] goods_infos;
			public device device;
		}
	}
	/// <summary>
	/// Shelter : 640
	/// </summary>
	public class removeToDevice : CProtocolBase
	{
	
		public long device_key;
		public int shelter_level_id;

		public removeToDevice()
		{
			 this.cmd = 640;
		}

		public class result : CResponseDataBase
		{
			public goods_info[] goods_infos;
			public device device;
		}
	}
	/// <summary>
	/// Shelter : 650
	/// </summary>
	public class doMoveToDevice : CProtocolBase
	{
	
		public int shelter_level_id;
		public int move_shelter_level_id;

		public doMoveToDevice()
		{
			 this.cmd = 650;
		}

		public class result : CResponseDataBase
		{
			public device device;
			public device move_device;
		}
	}
	/// <summary>
	/// Shelter : 660
	/// </summary>
	public class doGoodsDeviceFarming : CProtocolBase
	{
	
		public long device_key;

		public doGoodsDeviceFarming()
		{
			 this.cmd = 660;
		}

		public class result : CResponseDataBase
		{
			public device device;
			public goods_info[] goods_infos;
		}
	}
	/// <summary>
	/// Shelter : 665
	/// </summary>
	public class doGoodsDevicesFarming : CProtocolBase
	{
	
		public int device_type;

		public doGoodsDevicesFarming()
		{
			 this.cmd = 665;
		}

		public class result : CResponseDataBase
		{
			public device[] devices;
			public goods_info[] goods_infos;
		}
	}
	/// <summary>
	/// Shelter : 780
	/// </summary>
	public class trapIsActive : CProtocolBase
	{
		public long[] device_keys;
		public int goods_type;

		public trapIsActive()
		{
			 this.cmd = 780;
		}

		public class result : CResponseDataBase
		{
			public device[] devices;
			public goods_info[] goods_infos;
		}
	}
}
