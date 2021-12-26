using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DataTypeEnum;
using DataFileEnum;
using DataLoadLib.Global;
using LitJson;

public class SocialManager : MonoSingleton<SocialManager>
{
	private NMChannel						m_eSelectedChannel = NMChannel.EveryNetmarble;
	private Dictionary<string, CSocialData>	m_dicSocialData = null;
	private SocialBase m_cSocial = null;

	public bool IsInitialized 
	{ 
		get 
		{ 
			if(m_cSocial == null)
				return false;
			return m_cSocial.IsInitialized;
		} 
	}

	public NMChannel GetChannel
	{
		get
		{
			return m_eSelectedChannel;
		}
	}

	public override void Init()
	{
		base.Init();

		if(GlobalVariable._setting_info == null)
			ClientUtility.LoadSettingFile();
		
		m_dicSocialData = GlobalVariable._setting_info.SocialData;
		if(m_dicSocialData == null)
		{
			CreateSocialData();
		}

		SetAllDataClear();

		SetAllLog();
	}

	protected override void OnDestroy()
	{
		if(m_cSocial != null)
			m_cSocial.Clear();
		
		base.OnDestroy ();
	}

	public void ConnectAPI(NMChannel eChannel)
	{
		if(m_eSelectedChannel == eChannel)
		{
			return;
		}

		switch(eChannel)
		{
		case NMChannel.AppleGameCenter :
			m_cSocial = new SocialApple();
			break;
		case NMChannel.GooglePlus :
			m_cSocial = new SocialGoogle();
			break;
		}

		if(m_cSocial != null)
		{
			m_eSelectedChannel = eChannel;
			m_cSocial.ConnectAPI();
		}
	}

	public void DisConnectAPI()
	{
		if(m_cSocial != null || m_eSelectedChannel != NMChannel.EveryNetmarble)
		{
			NmSdkManager.Instance.DisconnectChannel(m_eSelectedChannel);
			m_cSocial.DisconnectAPI();
			m_cSocial = null;
			m_eSelectedChannel = NMChannel.EveryNetmarble;
		}
	}

	public void OpenAchievement()
	{
		if(m_cSocial == null)
			return;
		
		m_cSocial.OpenAchievement();
	}

	public void OpenLeaderboard()
	{
		if(m_cSocial == null)
			return;
		
		m_cSocial.OpenLeaderboard();
	}

	public void AddLeaderBoard(t_Accomplish eCheckType)
	{
		if(eCheckType == t_Accomplish.None)
			return;

		bool bChangeData = false;

		foreach(CSocialData cData in m_dicSocialData.Values)
		{
			if(!cData.bAchievement && cData.eCheckType == eCheckType)
			{
				bChangeData = true;

				cData.lCurValue += 1;

				if(m_cSocial != null)
				{
					m_cSocial.SetLeaderBoard(cData.strID, cData.lCurValue);
				}

				RTDebug.Log(cData.ToString());
			}
		}

		if(bChangeData)
		{
			ClientUtility.SaveSettingFIle();
		}
	}

	public void AddAchievementValue(t_Accomplish eCheckType, int nAddValue)
	{
		if(eCheckType == t_Accomplish.None)
			return;
		
		bool bChangeData = false;

		foreach(CSocialData cData in m_dicSocialData.Values)
		{
			if(cData.bAchievement && cData.eCheckType == eCheckType && !cData.bClear)
			{
				bChangeData = true;

				if(nAddValue == 0)
				{
					SetAchievementEnd(cData);
				}
				else if(cData.lCheckValue <= cData.lCurValue + nAddValue)
				{
					SetAchievementEnd(cData);
				}
				else
				{
					cData.lCurValue += nAddValue;
				}

				RTDebug.Log(cData.ToString());
			}
		}

		if(bChangeData)
		{
			ClientUtility.SaveSettingFIle();
		}
	}

	private void SetAchievementEnd(CSocialData cData)
	{
		cData.bClear = true;
		if(m_cSocial != null)
		{
			m_cSocial.SetAchievementEnd(cData.strID);
		}
	}

	private void CreateSocialData()
	{
		Dictionary<int, TableInfo> dicTable = DataManager.GetInstance().GetDicDataFile(DATA_FILE_ENUM.GameCenter_Achieve);
		if(dicTable == null)
		{
			Debug.LogError("TableParsingError : " + DataFileEnum.DATA_FILE_ENUM.GameCenter_Achieve.ToString());
			return;
		}

		m_dicSocialData = new Dictionary<string, CSocialData>();
		foreach(TableInfo info in dicTable.Values)
		{
			CSocialData cData = new CSocialData();
			cData.strID = info.GetStrValue((int)GameCenter_AchieveIndex.ID).Trim();
			cData.eCheckType = (t_Accomplish)info.GetIntValue((int)GameCenter_AchieveIndex.Accomplish_Type);
			cData.bAchievement = info.GetIntValue((int)GameCenter_AchieveIndex.Achieve_Type) == 2 ? true : false;
			cData.lCheckValue = (long)info.GetIntValue((int)GameCenter_AchieveIndex.Record_Value);
			m_dicSocialData.Add(cData.strID, cData);
		}

		GlobalVariable._setting_info.SocialData = m_dicSocialData;

		ClientUtility.SaveSettingFIle();
	}

	private void SetAllLog()
	{
		if(m_dicSocialData == null)
			return;
		
		foreach(CSocialData cData in m_dicSocialData.Values)
		{
			RTDebug.Log(cData.ToString());
		}
	}

	private void SetAllDataClear()
	{
		foreach(CSocialData cData in m_dicSocialData.Values)
		{
			cData.ClearData();
		}
	}

	private void CBConnectAPI(string strArg)
	{
		switch(m_eSelectedChannel)
		{
		case NMChannel.GooglePlus :
			if(m_cSocial is SocialGoogle)
			{
				SocialGoogle cSocial = m_cSocial as SocialGoogle;
				cSocial.CBConnectAPI(strArg);
			}
			break;
		}

		//Do Call Leaderboard score from API
	}

	private void CBLoadAchievements(string strArg)
	{
		if(m_eSelectedChannel == NMChannel.GooglePlus && m_cSocial is SocialGoogle)
		{
			SocialGoogle cSocial = m_cSocial as SocialGoogle;
			cSocial.CBLoadAchievements(strArg);
		}
	}

	private void CBUnlockAchievement(string strID)
	{
		if(m_eSelectedChannel == NMChannel.GooglePlus && m_cSocial is SocialGoogle)
		{
			SocialGoogle cSocial = m_cSocial as SocialGoogle;
			cSocial.CBUnlockAchievement(strID);
		}
	}

	private void CBIncrementAchievement(string strID)
	{
		if(m_eSelectedChannel == NMChannel.GooglePlus && m_cSocial is SocialGoogle)
		{
			SocialGoogle cSocial = m_cSocial as SocialGoogle;
			cSocial.CBIncrementAchievement(strID);
		}
	}

	public void CBUnlockAchievementError(string strID)
	{
		if(m_dicSocialData.ContainsKey(strID))
		{
			m_dicSocialData[strID].bClear = false;
		}
	}

	private void CBIntent(string strCode)
	{
		int nResultCode = int.Parse(strCode);
		if(nResultCode > 0)
		{
			DisConnectAPI();
		}
	}
}

