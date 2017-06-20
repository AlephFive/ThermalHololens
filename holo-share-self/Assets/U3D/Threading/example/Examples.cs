using UnityEngine;
using System.Collections;
using U3D.Threading.Tasks;
using UnityEngine.UI;
using System.Collections.Generic;

public class Examples : MonoBehaviour 
{
	public Text log;
	Button[] m_buttons;

	void Start()
	{
		U3D.Threading.Dispatcher.Initialize ();
		m_buttons = FindObjectsOfType<Button> ();
	}
	void EnableButtons()
	{
		foreach (Button b in m_buttons)
			b.interactable = true;
	}
	void DisableButtons()
	{
		foreach (Button b in m_buttons)
			b.interactable = false;
	}
	System.Random m_rnd= new System.Random(); // UnityEngine.Random needs to be executed in main thread (!!)
	void MockUpSomeDelay()
	{
		System.Threading.Thread.Sleep (1000 * (m_rnd.Next(4) + 1));
	}
	void CheckTaskExecution(Task t)
	{
		if (t.IsFaulted) 
		{
			log.text+= "<color=#550000>Error executing task: " + t.Exception + "</color>\n";
		}
		else
		{
			log.text+= "<color=#005500>Tasks executed successfully</color>\n";
		}
		EnableButtons();
	}

	public void DispatchToMainThread()
	{
		DisableButtons ();
		log.text += "<b>***** DispatchToMainThread()</b>\n";
		Task.Run (() => {
			// log.text += "Testing\n"; => this would cause an exception, we are not in the main thread
			Task.RunInMainThread(() => log.text += "Testing in main thread\n");
		}).ContinueInMainThreadWith(CheckTaskExecution);
		EnableButtons ();
	}
	public void CreateThread()
	{
		DisableButtons ();
		log.text += "<b>***** CreateThread()</b>\n";
		Task.Run (() => {
			// [...] Code to be executed in auxiliary thread
			MockUpSomeDelay();
		}).ContinueInMainThreadWith(CheckTaskExecution);
	}
	public void CreateThreadReturningValue()
	{
		DisableButtons ();
		log.text += "<b>***** CreateThreadReturningValue()</b>\n";
		Task<int>.Run (() => {
			// [...] Code to be calculate some data in thread
			int randomData= m_rnd.Next(1000);
			Debug.Log("Random data: " + randomData);
			return randomData;
		}).ContinueInMainThreadWith((t) => {
			log.text+= "Result: " + t.Result + "\n";
			CheckTaskExecution(t);
		});
	}

	Task m_taskToAbort= null;
	public void CreateAbortableThread(Button b)
	{
		DisableButtons ();
		b.interactable = true;
		if (m_taskToAbort == null) 
		{
			b.GetComponentInChildren<Text> ().text = "Cancel";
			log.text += "<b>***** CreateAbortableThread()</b>\n";
			m_taskToAbort = Task.Run (() => {
				try
				{
					// [...] Code that supports to be aborted
					Debug.Log("0/3");
					MockUpSomeDelay();
					Debug.Log("1/3");
					MockUpSomeDelay();
					Debug.Log("2/3");
					MockUpSomeDelay();
					Debug.Log("3/3");
					Debug.Log("Thread finished without being aborted");
				}
				catch(System.Threading.ThreadAbortException tae)
				{
					Debug.Log ("Thread was aborted before finishing");
					throw tae; // rethrow if you want the tas to be set as aborted
				}
			});
			m_taskToAbort.ContinueInMainThreadWith ((t) => {
				if(t.IsAborted)
					log.text+= "Thread was aborted before finishing\n";
				else
					log.text+= "Thread finished without being aborted\n";

				CheckTaskExecution(t);
				m_taskToAbort= null;				
				b.GetComponentInChildren<Text> ().text = "Create abortable thread";
			});
		} 
		else 
		{
			m_taskToAbort.AbortThread();
		}
	}
	public void WaitForThread()
	{	// Not recommended to do in the main thread, it will block the execution
		DisableButtons ();
		log.text += "<b>***** WaitForThread()</b>\n";
		Task.Run (() => {
			// [...] Code to be executed in auxiliary thread, and launches a second auxiliary thread
			Task t = Task.Run (() => {
				// [...] Code to be executed in auxiliary thread
				MockUpSomeDelay ();
				Debug.Log("Task is finished, parent thread will be unblocked");
			});

			// [...] And waits for that thread to finish
			Debug.Log("Will wait for task");
			t.Wait ();
			Debug.Log("Task finished execution");
		}).ContinueInMainThreadWith (CheckTaskExecution);
	}
	int m_finishedTasksCounter;
	Task CreateTask(int total)
	{
		return Task.Run (() => {
			MockUpSomeDelay ();
			lock(this)
			{
				m_finishedTasksCounter++;
			}
			Debug.Log ("Task " + m_finishedTasksCounter + "/" + total + " finished");
		});
	}
	public void WhenAll()
	{	
		DisableButtons ();
		log.text += "<b>***** WhenAll()</b>\n";
		List<Task> tasks = new List<Task> ();

		m_finishedTasksCounter = 0;
		int nbOfTasks = m_rnd.Next (4) + 6;
		for (int i=0; i< nbOfTasks; i++) 
		{
			// Create a list of tasks
			Task t = CreateTask (nbOfTasks);
			tasks.Add (t);
		}
		// this task will wait for all task to finish
		Task.WhenAll (tasks).ContinueInMainThreadWith (CheckTaskExecution);
	}
	public void WhenAny()
	{	
		DisableButtons ();
		log.text += "<b>***** WhenAny()</b>\n";
		List<Task> tasks = new List<Task> ();
		
		m_finishedTasksCounter = 0;
		int nbOfTasks = m_rnd.Next (4) + 10;
		for (int i=0; i< nbOfTasks; i++) 
		{
			// Create a list of tasks
			Task t = CreateTask (nbOfTasks);
			tasks.Add (t);
		}
		// this task will wait for all task to finish
		Task.WhenAny (tasks).ContinueInMainThreadWith ((t) => {
			t.AbortThread();
			CheckTaskExecution(t);
		});
	}
	public void WrongLoopUsage()
	{
		DisableButtons ();
		log.text += "<b>***** WrongLoopUsage()</b>\n";
		List<Task> tasks = new List<Task> ();
		for(int i=0; i<10; i++)
		{
			tasks.Add(Task.RunInMainThread(()=> {
				// this code will execute 10 times in the main thread, value will always be 9 for all the executions
				log.text+= i.ToString() + "\n";
			}));
		}
		Task.WhenAll (tasks).ContinueInMainThreadWith (CheckTaskExecution);
	}
	public void RightLoopUsage()
	{
		DisableButtons ();
		log.text += "<b>***** RightLoopUsage()</b>\n";
		List<Task> tasks = new List<Task> ();
		for(int i=0; i<10; i++)
		{
			int value = i;
			tasks.Add(Task.RunInMainThread(()=> {
				// now value will retain its value in each task
				log.text+= value.ToString() + "\n";
			}));
		}
		Task.WhenAll (tasks).ContinueInMainThreadWith (CheckTaskExecution);
	}
	public void AdvancedTaskControl()
	{
		DisableButtons ();
		log.text += "<b>***** AdvancedTaskControl()</b>\n";
		AdvancedTask().ContinueInMainThreadWith (CheckTaskExecution);
	}
	Task AdvancedTask()
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool> ();

		Task.Run (() => {
			// we can control the state of the returned task from any other thread
			ComplicatedThreadSchema (tcs);
		});

		return tcs.Task;
	}
	void ComplicatedThreadSchema(TaskCompletionSource<bool> tcs)
	{	// here we can implement anything: 
		// - a binary tree search,
		// - concatenated requests to a server with logic deciding if more requests are needed
		// - some complicated path finding
		// - etc..
		// the rest of our logic will be decoupled of it and just waiting for the task to be completed
		// Check LeaderboardTest.GetCompleteLeaderboard
		int action= m_rnd.Next(10);
		if(action==0)
		{
			Debug.Log("Thread schema finished");
			tcs.SetResult(true); // finish the thread
		}
		else
		{
			Debug.Log("Thread schema continues...");
			Task.Run(() => ComplicatedThreadSchema(tcs));
		}
	}
}
