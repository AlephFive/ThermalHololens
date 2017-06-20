using UnityEngine;
using UnityEngine.UI;
using System;
using U3D.Threading;
using U3D.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

public class LeaderboardTest : MonoBehaviour 
{
	public void Start () 
	{
		Dispatcher.Initialize();
	}
	public Text text;
	void CleanLog()
	{
		lock (text) 
		{
			text.text = "";
		}
	}
	void WriteLog(string fmt, params object[] pars)
	{
		lock(text)
		{
			text.text += string.Format("{0}\t\t{1}\n", DateTime.Now, string.Format(fmt, pars));
		}
	}

	bool m_executing = false;
	public void Execute(Button sender)
	{
		Image senderImage = sender.GetComponent<Image> ();
		Text senderText = sender.GetComponentInChildren<Text> ();
		if (m_executing) 
		{
			m_stopProcessing= true;
		} 
		else 
		{
			if(m_stopProcessing) return;
			m_executing= true;

			m_stopProcessing= false;
			CleanLog ();
			senderImage.color = Color.red;
			senderText.text = "Stop";

			GetCompleteLeaderboard(
				(pos, name) =>
				{
					WriteLog("{0} - {1}", pos, name);
				}
				).ContinueInMainThreadWith((t) => {

				senderImage.color = Color.white;
				senderText.text = "Run leaderboard tests";
				m_executing= false;

				if(t.IsFaulted)
				{
					Exception e = t.Exception.InnerExceptions[0];
					while (e is U3D.AggregateException) e = ((U3D.AggregateException)e).InnerExceptions[0];					
					WriteLog("--- Error getting leaderboard {0}", e.Message);
				}
				else
				{
					WriteLog("--- Finished getting leaderboard");
				}
			});
		}
	}

	bool m_stopProcessing;
	Task GetCompleteLeaderboard(Action<int, string> intermediateResult)
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool> ();
		GetCompleteLeaderboardAux (0, tcs, intermediateResult);
		return tcs.Task;
	}
	void GetCompleteLeaderboardAux(int offset, TaskCompletionSource<bool> tcs, Action<int, string> intermediateResult)
	{
		if (m_stopProcessing) 
		{
			m_stopProcessing= false;
			tcs.SetError(new Exception("User cancelled"));
		}
		else
		{
			StartCoroutine(ConnectToAPI(offset, tcs, intermediateResult));
		}
	}
	const int queryLimit= 10;
	IEnumerator ConnectToAPI(int offset, TaskCompletionSource<bool> tcs, Action<int, string> intermediateResult)
	{
		yield return StartCoroutine(YOUR_FAVORITE_API_GetLeaderBoardUsingWWW (offset, queryLimit));

		SortedList<int, string> result = YOUR_FAVORITE_API_GetResultsFromCall ();

		if(result== null)
		{	// mockup an error in the API
			tcs.SetError(new Exception("API returns NULL"));
		}

		foreach(int k in result.Keys)
		{
			intermediateResult(k, result[k]);
		}
		if (result.Count == queryLimit) 
		{
			GetCompleteLeaderboardAux(offset + queryLimit, tcs, intermediateResult);
		}
		else
		{
			tcs.SetResult(true);
		}
	}

	#region API mock up (WWW based)
	const int secondsDelay = 1;
	int m_currentOffset;
	int m_currentLimit;
	IEnumerator YOUR_FAVORITE_API_GetLeaderBoardUsingWWW (int offset, int limit)
	{
		m_currentOffset = -1;
		yield return new WaitForSeconds(secondsDelay);
		m_currentOffset = offset;
		m_currentLimit = limit;
	}

	const int nbOfRegistersTotal = 303;
	SortedList<int, string> YOUR_FAVORITE_API_GetResultsFromCall ()
	{
		SortedList<int, string> ret = new SortedList<int, string> ();
		for(int i= 0; i< m_currentLimit; i++)
		{
			int currentRegister= m_currentOffset + i;
			if(currentRegister > nbOfRegistersTotal)
				break;
			ret.Add(currentRegister, "Player " + UnityEngine.Random.Range(0, nbOfRegistersTotal));
		}
		return ret;
	}
	#endregion
}