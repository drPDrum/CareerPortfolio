package com.npicsoft.drum.gpsm;

import com.unity3d.player.*;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.api.GoogleApiClient;
import com.google.android.gms.common.api.PendingResult;
import com.google.android.gms.common.api.ResultCallback;
import com.google.android.gms.games.Games;
import com.google.android.gms.games.Player;
import com.google.android.gms.games.achievement.Achievement;
import com.google.android.gms.games.achievement.AchievementBuffer;
import com.google.android.gms.games.achievement.Achievements.LoadAchievementsResult;
import com.google.android.gms.games.achievement.Achievements.UpdateAchievementResult;
import com.google.android.gms.games.leaderboard.Leaderboards.SubmitScoreResult;

public class GPSManager implements GoogleApiClient.ConnectionCallbacks, GoogleApiClient.OnConnectionFailedListener
{
	private static GPSManager	m_Instance = null;
	private static String		LOG_TAG = "UnityDrumTest";
	private static int			ACHIEVEMENT_CODE = 1004;
	private static int			LEADERBODE_CODE = 2004;
	private Activity			m_activityMain = null;
	private Context 			m_contextMain = null;
	private GoogleApiClient		m_GoogleApi = null;
	
	public GPSManager()
	{
		GPSManager.m_Instance = this;
	}
	
	public static GPSManager Instance()
	{
		if(m_Instance == null)
		{
			m_Instance = new GPSManager();
		}
		return m_Instance;
	}
	
	public void SetInitial(Context context, Activity activity)
	{
		Log.d(LOG_TAG, "SetInitial Called");
		this.m_contextMain = context;
		this.m_activityMain = activity;
		this.m_GoogleApi = new GoogleApiClient.Builder(m_contextMain)
				.addConnectionCallbacks(this)
				.addOnConnectionFailedListener(this)
				.addApi(Games.API).addScope(Games.SCOPE_GAMES)				
				// add other APIs and scopes here as needed
				.build();
		Log.d(LOG_TAG, "JavaFDrumTest Called - End");		
	}
	
	@Override
	public void onConnectionFailed(ConnectionResult arg0) 
	{
	}

	@Override
	public void onConnected(Bundle arg0) 
	{
		UnityPlayer.UnitySendMessage("Singleton_SocialManager", "CBConnectAPI", "true");
	}

	@Override
	public void onConnectionSuspended(int arg0) 
	{
	}

	public void ConnectAPI()
	{
		m_GoogleApi.connect();		
	}
	
	public void DisconnectAPI()
	{
		m_GoogleApi.disconnect();
	}
	
	public void CheckCurrentPlayer()
	{
		if(!m_GoogleApi.isConnected())
		{
			SendConnectionLost();
			return;
		}
		
		Player player = Games.Players.getCurrentPlayer(m_GoogleApi);
	}
	
	public void OpenAchievementIntent()
	{
		if(!m_GoogleApi.isConnected())
		{
			SendConnectionLost();
			return;
		}
		
		Intent intent = Games.Achievements.getAchievementsIntent(m_GoogleApi);
		
		m_activityMain.startActivityForResult(intent, ACHIEVEMENT_CODE);				
	}
	
	public void OpenLeaderboardIntent()
	{
		if(!m_GoogleApi.isConnected())
		{
			SendConnectionLost();
			return;
		}
		
		CheckCurrentPlayer();
		
		try
		{
			Intent intent = Games.Leaderboards.getAllLeaderboardsIntent(m_GoogleApi);
			
			
			m_activityMain.startActivityForResult(intent, LEADERBODE_CODE);
		}
		catch(Exception e)
		{
			Log.d(LOG_TAG, e.toString());
		}
	}
	
	public Intent GetLeaderBoardIntent()
	{
		return Games.Leaderboards.getAllLeaderboardsIntent(m_GoogleApi);
	}
	
	public Intent GetAchievementIntent()
	{
		return Games.Achievements.getAchievementsIntent(m_GoogleApi);
	}
	
	public void LoadAchievements()
	{
		if(!m_GoogleApi.isConnected())
		{
			SendConnectionLost();
			return;
		}
		
		PendingResult<LoadAchievementsResult> result = Games.Achievements.load(m_GoogleApi, true);
		result.setResultCallback
		(
			new ResultCallback<LoadAchievementsResult>() 
			{
	            @Override
	            public void onResult(LoadAchievementsResult loadAchievementsResult) 
	            {	            	
	            	CBLoadAchievements(loadAchievementsResult);
	            }
			}
		);
	}
	
	public void UnLockAchievement(String strID)
	{
		if(!m_GoogleApi.isConnected())
		{
			SendConnectionLost();
			return;
		}
		
		Log.d(LOG_TAG, "UnLockAchievement : " + strID);
		PendingResult<UpdateAchievementResult> result = Games.Achievements.unlockImmediate(m_GoogleApi, strID);
		result.setResultCallback
		(
			new ResultCallback<UpdateAchievementResult>()
			{
				@Override
				public void onResult(UpdateAchievementResult updateAchievementsResult) 
				{
					CBUnlockAchievement(updateAchievementsResult);
				}
				
			}
		);
	}
		
	public void IncrementLeaderBoard(String strID, long lValue)
	{
		if(!m_GoogleApi.isConnected())
		{
			SendConnectionLost();
			return;
		}
		
		PendingResult<SubmitScoreResult> result = Games.Leaderboards.submitScoreImmediate(m_GoogleApi, strID, lValue);
		result.setResultCallback
		(
			new ResultCallback<SubmitScoreResult>()
			{
				@Override
				public void onResult(SubmitScoreResult submitScoreResult) 
				{
					CBSubmitScoreResult(submitScoreResult);
				}
			}
		);
	}
	
	private void SendConnectionLost()
	{
		UnityPlayer.UnitySendMessage("Singleton_SocialManager", "CBConnectionLost", "");		
	}
	
	private void CBLoadAchievements(LoadAchievementsResult loadAchievementsResult)
	{
		AchievementBuffer buffer = loadAchievementsResult.getAchievements();
		
		String strJson = "{\"Achievements\":[{";
		for (int i = 0 ; i < buffer.getCount() ; i++)
		{
			if(buffer.get(i) == null)
			{
				Log.d(LOG_TAG, "CBAchievementsLoad[" + i + "] = Null");
				continue;
			}
			
			strJson += "\"ID\":\""
					+ buffer.get(i).getAchievementId()
					+ "\",\"Name\":\"" 
					+ buffer.get(i).getName()
					+ "\",\"State\":" 
					+ buffer.get(i).getState()
					+ ",\"Type\":" 
					+ buffer.get(i).getType();
			
			if(buffer.get(i).getType() == Achievement.TYPE_INCREMENTAL)
			{
				strJson += ",\"CurStep\":" 
						+ buffer.get(i).getCurrentSteps()
						+ ",\"CurFormattedStep\":" 
						+ buffer.get(i).getFormattedCurrentSteps()
						+ ",\"TotalFormattedStep\":" 
						+ buffer.get(i).getFormattedTotalSteps();
			}
			
			if (i < buffer.getCount() - 1) 
			{
				strJson += "},{";
			} 
			else 
			{
				strJson += "}]}";
			}
		}		
		
		UnityPlayer.UnitySendMessage("Singleton_SocialManager", "CBLoadAchievements", strJson);
	}
	
	private void CBUnlockAchievement(UpdateAchievementResult updateAchievementsResult)
	{
		if(updateAchievementsResult == null)
			return;
				
		if("STATUS_OK" == updateAchievementsResult.getStatus().getStatusMessage())
		{
			UnityPlayer.UnitySendMessage("Singleton_SocialManager", "CBUnlockAchievement", updateAchievementsResult.getAchievementId());			
		}
		else
		{
			UnityPlayer.UnitySendMessage("Singleton_SocialManager", "CBUnlockAchievementError", updateAchievementsResult.getAchievementId());
		}
			
		
		updateAchievementsResult.getStatus().getStatusCode();
		
	}
	
	private void CBSubmitScoreResult(SubmitScoreResult submitScoreResult)
	{
		if(submitScoreResult == null)
			return;
		
		UnityPlayer.UnitySendMessage("Singleton_SocialManager", "CBUnlockAchievement", submitScoreResult.getScoreData().getLeaderboardId());
	}
	
	public void CBIntent(int nResultCode)
	{
		if(nResultCode > 0)
		{
			UnityPlayer.UnitySendMessage("Singleton_SocialManager", "CBIntent", String.valueOf(nResultCode));
		}		
	}
}
