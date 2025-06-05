// Unity Editor Coroutine
// Author: Jingxuan Wang
// Created: 2014/10/20
// https://gist.github.com/JingxuanWang/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

namespace Midas.Tools.Editor
{
	/// <summary>
	/// This class ties into the update system of the unity editor to build
	/// a Coroutine type system.  This lets us do long lasting function code without
	/// freezing up the unity editor in a way that will be familiar to those who are
	/// used to unity.
	/// </summary>
	public static class EditorCoroutineRunner
	{
		/// <summary>
		/// Used to tell the editor coroutine runner to wait for a single frame before continuing.
		/// </summary>
		/// <remarks>
		/// Intentionally empty because we just need something to reference, doesn't need any implementation right now.
		/// </remarks>
		public class WaitForEndOfFrame
		{
		}

		/// <summary>
		/// Used to tell the editor coroutine runner to wait for N frames before continuing.
		/// </summary>
		public class WaitForFrames
		{
			/// <summary>
			/// Number of frames to delay for.
			/// </summary>
			public int FrameCount { get; private set; }

			/// <summary>
			/// Initialize an instance of this structure with a number of frames.
			/// </summary>
			/// <param name="frameCount">Number of frames to delay for.</param>
			public WaitForFrames(int frameCount)
			{
				FrameCount = frameCount;
			}
		}

		/// <summary>
		/// Data used when starting a new Coroutine
		/// </summary>
		private struct EditorCoroutineData
		{
			/// <summary>
			/// A enumerator of the coroutine
			/// </summary>
			public IEnumerator Iterator;

			/// <summary>
			/// Action to invoke when coroutine completes
			/// </summary>
			public Action CompleteAction;
		};

		/// <summary>
		/// Keep track of what we are waiting for when delaying for frames.
		/// </summary>
		private enum EndOfFrameDelay
		{
			None,
			Waiting,
			Reached,
			WaitingCount,
		}

		/// <summary>
		/// A function that will be run in a way similar to coroutines in unity
		/// </summary>
		private sealed class EditorCoroutine : IEnumerator
		{
			private readonly Stack<IEnumerator> executionStack;

			private readonly Stopwatch timer = new Stopwatch();

			private float delayTime;

			private Action onCompleteAction;

			private int delayCount;

			/// <summary>
			/// What type of frame delay are we waiting for?
			/// </summary>
			public EndOfFrameDelay WaitingForEndOfFrame = EndOfFrameDelay.None;

			/// <summary>
			/// How many frames have passed since we started delaying.
			/// </summary>
			public int FrameCount;

			/// <summary>
			/// Stop executing this coroutine.
			/// </summary>
			public bool CancelExecution;

			/// <summary>
			/// Internal class that is used by EditorCoroutineRunner to track the
			/// coroutines that we are adding to the update list.
			/// </summary>
			/// <param name="iterator">IEnumerator Function that the class wraps around.</param>
			/// <param name="completeAction">action to call when coroutine completes.</param>
			public EditorCoroutine(IEnumerator iterator, Action completeAction)
			{
				delayTime = 0f;
				timer.Reset();
				timer.Start();
				onCompleteAction = completeAction;
				executionStack = new Stack<IEnumerator>();
				executionStack.Push(iterator);
			}

			/// <summary>
			/// Use MoveNext on an IEnumerator function to have partial functions called.
			/// </summary>
			/// <returns>Return true if we can continue using the iterator, false if not.</returns>
			public bool MoveNext()
			{
				var moveOn = false;

				switch (WaitingForEndOfFrame)
				{
					// Frame delay needs to come before testing for the timer.
					// Because it will always have been reached, unless we are specifically waiting for the timer.
					case EndOfFrameDelay.Reached:
						WaitingForEndOfFrame = EndOfFrameDelay.None;
						moveOn = true;
						break;
					case EndOfFrameDelay.WaitingCount:
					{
						if (FrameCount >= delayCount)
						{
							WaitingForEndOfFrame = EndOfFrameDelay.None;
							moveOn = true;
						}

						break;
					}
					default:
					{
						if (timer.Elapsed.Seconds >= delayTime)
						{
							timer.Stop();
							moveOn = true;
						}

						break;
					}
				}

				return CancelExecution switch
				{
					false when moveOn => GetNext(),
					true => false,
					_ => true
				};
			}

			/// <summary>
			/// Find the next IEnumerator function to have partial functions called.
			/// </summary>
			/// <returns>Return true if we can continue using the iterator, false if not.</returns>
			private bool GetNext()
			{
				var i = executionStack.Peek();
				try
				{
					if (i.MoveNext())
					{
						var result = i.Current;
						switch (result)
						{
							case IEnumerator enumerator:
								executionStack.Push(enumerator);
								break;
							case WaitForEndOfFrame _:
								WaitingForEndOfFrame = EndOfFrameDelay.Waiting;
								break;
							case WaitForFrames frames:
								delayCount = frames.FrameCount;
								FrameCount = 0;

								WaitingForEndOfFrame = EndOfFrameDelay.WaitingCount;
								break;
							case float f:
								delayTime = f;
								timer.Reset();
								timer.Start();
								break;
						}

						return true;
					}

					if (executionStack.Count > 1)
					{
						executionStack.Pop();
						return true;
					}

					InvokeCompleteCallback();
					return false;
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError("Unhandled Exception: " + ex.Message + "\nStack:\n" + ex.StackTrace);
				}

				return true;
			}

			/// <summary>
			/// This is unsupported.
			/// </summary>
			public void Reset()
			{
				throw new NotSupportedException("This Operation Is Not Supported.");
			}

			/// <summary>
			/// Get current coroutine being run.
			/// </summary>
			public object Current
			{
				get { return executionStack.Peek().Current; }
			}

			/// <summary>
			/// Is iterator on the functions stack.
			/// </summary>
			/// <param name="iterator">Which iterator to search for.</param>
			/// <returns>Return true if the function is in the execution stack, false otherwise.</returns>
			public bool Find(IEnumerator iterator)
			{
				return executionStack.Contains(iterator);
			}

			/// <summary>
			/// Calls the complete action.
			/// </summary>
			private void InvokeCompleteCallback()
			{
				if (onCompleteAction != null)
				{
					var action = onCompleteAction;
					onCompleteAction = null;
					action.Invoke();
				}
			}
		}

		private static List<EditorCoroutine> editorCoroutineList;
		private static List<EditorCoroutineData> buffer;

		/// <summary>
		/// Add coroutine to Editor.  This function will get called every time the editor
		///  gets updated.
		/// </summary>
		/// <param name="iterator">the IEnumerator function that someone wants run in chunks. </param>
		/// <param name="completeCallback">Action to call when the coroutine completes. </param>
		/// <returns>The functions iterator.</returns>
		public static IEnumerator StartEditorCoroutine(IEnumerator iterator, Action completeCallback = null)
		{
			if (editorCoroutineList == null)
			{
				editorCoroutineList = new List<EditorCoroutine>();
			}

			if (buffer == null)
			{
				buffer = new List<EditorCoroutineData>();
			}

			if (editorCoroutineList.Count == 0)
			{
				EditorApplication.update += Update;
			}

			// add iterator to buffer first
			buffer.Add(new EditorCoroutineData
			{
				Iterator = iterator,
				CompleteAction = completeCallback
			});

			return iterator;
		}

		/// <summary>
		/// Cancel a specific coroutine that is running.
		/// </summary>
		/// <param name="iterator">The iterator which is being run.</param>
		public static void CancelEditorCoroutine(IEnumerator iterator)
		{
			foreach (var editorCoroutine in editorCoroutineList)
			{
				if (editorCoroutine.Find(iterator))
				{
					editorCoroutine.CancelExecution = true;
				}
			}
		}

		/// <summary>
		/// Return whether or not the passed <see cref="IEnumerator"/> is already in the list.
		/// </summary>
		/// <param name="iterator"><see cref="IEnumerator"/> to look for.</param>
		/// <returns>True if already present."/></returns>
		private static bool Find(IEnumerator iterator)
		{
			// If this iterator is already added
			// Then ignore it this time
			foreach (var editorCoroutine in editorCoroutineList)
			{
				if (editorCoroutine.Find(iterator))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Update state of all coroutines.
		/// </summary>
		private static void Update()
		{
			// EditorCoroutine execution may append new iterators to buffer
			// Therefore we should run EditorCoroutine first
			editorCoroutineList.RemoveAll(coroutine => coroutine.MoveNext() == false);

			// Anything that is waiting for the fame update to occur.
			editorCoroutineList.ForEach(coroutine =>
			{
				switch (coroutine.WaitingForEndOfFrame)
				{
					case EndOfFrameDelay.Waiting:
						coroutine.WaitingForEndOfFrame = EndOfFrameDelay.Reached;
						break;
					case EndOfFrameDelay.WaitingCount:
						coroutine.FrameCount++;
						break;
				}
			});

			// If we have iterators in buffer
			if (buffer.Count > 0)
			{
				foreach (var data in buffer)
				{
					// If this iterators not exists
					if (!Find(data.Iterator))
					{
						// Added this as new EditorCoroutine
						editorCoroutineList.Add(new EditorCoroutine(data.Iterator, data.CompleteAction));
					}
				}

				// Clear buffer
				buffer.Clear();
			}

			// If we have no running EditorCoroutine
			// Stop calling update anymore
			if (editorCoroutineList.Count == 0)
			{
				EditorApplication.update -= Update;
			}
		}
	}
}