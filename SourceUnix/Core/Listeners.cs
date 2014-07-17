#region Copyright
/*
* Copyright (c) 2005,2006,2007, OpenMI Association
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the OpenMI Association nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY "OpenMI Association" ``AS IS'' AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL "OpenMI Association" BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion 
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.IO;
//using System.Windows.Forms;
using Oatc.OpenMI.Sdk.Backbone;
using OpenMI.Standard;
using Oatc.OpenMI.Sdk.DevelopmentSupport;
using Oatc.OpenMI.Gui.Core;

namespace Oatc.OpenMI.Gui.Core
{
	/// <summary>
	/// Listener used to log simulation progress to text file.
	/// </summary>
	public class LogFileListener: IListener
	{			
		EventType[] _acceptedEventTypes;
		StreamWriter _writer;

		/// <summary>
		/// Creates new instance of <see cref="LogFileListener">LogFileListener</see> which doesn't listen any
		/// event type. 
		/// </summary>
		public LogFileListener()
		{
			_acceptedEventTypes = new EventType[0];
		}


		/// <summary>
		/// Creates new instance of <see cref="LogFileListener">LogFileListener</see>.
		/// </summary>
		/// <param name="listenedEvents">Listened event types.</param>
		/// <param name="filename">Path to text file for logging.</param>
		/// <remarks>See <see cref="Initialize">Initialize</see> for more detail.</remarks>
		public LogFileListener( bool[] listenedEvents, string filename )
		{
			Initialize( listenedEvents, filename );
		}


		/// <summary>
		/// Closes text file for logging, if any.
		/// </summary>
		~LogFileListener()
		{
			if( _writer!=null )
			{
				_writer.Close();
				_writer = null;
			}
		}


		/// <summary>
		/// Initializes this listener to log events to text file.
		/// </summary>
		/// <param name="listenedEvents"><c>bool</c> array describing which event types should be listened.</param>
		/// <param name="filename">Path to text file for logging.</param>
		/// <remarks><c>listenedEvents</c> must have exactly
		/// <see cref="EventType.NUM_OF_EVENT_TYPES">EventType.NUM_OF_EVENT_TYPES</see> elements.
		/// See <see cref="EventType">EventType</see> for more detail.</remarks>
		public void Initialize( bool[] listenedEvents, string filename )
		{
			if( listenedEvents.Length != (int)EventType.NUM_OF_EVENT_TYPES )
				throw( new ArgumentException("Length of this array must be same as EventType.NUM_OF_EVENT_TYPES", "listenedEvents") );
			
			ArrayList acceptedEventTypes = new ArrayList();
			for( int i=0; i<listenedEvents.Length; i++ )
				if( listenedEvents[i] )
					acceptedEventTypes.Add( (EventType)i );
			_acceptedEventTypes = (EventType[])acceptedEventTypes.ToArray( typeof(EventType) );

			// open writer only if there is something to listen			
			if( _acceptedEventTypes.Length > 0 )
			{
				_writer = new StreamWriter( filename, false, Encoding.Unicode );
				_writer.AutoFlush = true;
			}
			else
			{
				_writer = null;
			}
		}
		

		/// <summary>
		/// Get accepted event type.
		/// </summary>
		/// <param name="acceptedEventTypeIndex">Index of accepted event type.</param>
		/// <returns>Returns accepted <see cref="EventType">EventType</see>.</returns>
		/// <remarks>See <see cref="IListener.GetAcceptedEventType">IListener.GetAcceptedEventType</see>
		/// for more detail.</remarks>
		public EventType GetAcceptedEventType(int acceptedEventTypeIndex)
		{
			return( _acceptedEventTypes[acceptedEventTypeIndex] );
		}


		/// <summary>
		/// Get accepted event type count.
		/// </summary>
		/// <returns>Returns number of accepted <see cref="EventType">EventType</see>.</returns>
		/// <remarks>See <see cref="IListener.GetAcceptedEventTypeCount">IListener.GetAcceptedEventTypeCount</see>
		/// for more detail.</remarks>
		public int GetAcceptedEventTypeCount()
		{
			return( _acceptedEventTypes.Length );
		}


		/// <summary>
		/// Logs one event to text file.
		/// </summary>
		/// <param name="Event">Event to be logged.</param>
		/// <remarks>See <see cref="IListener.OnEvent">IListener.OnEvent</see>
		/// for more detail.</remarks>
		public void OnEvent(IEvent Event)
		{
			if( _writer!=null )
			{
				_writer.WriteLine( Utils.EventToString(Event) );
			
				if( Event==CompositionManager.SimulationFinishedEvent
					|| Event==CompositionManager.SimulationFailedEvent )
				{
					_writer.Close();
					_writer = null;
				}
			}
		}		
	}



	/// <summary>
	/// Listener used to write events to console.
	/// </summary>
	/// 
	public class ConsoleListener: IListener
	{
		EventType[] _acceptedEventTypes;

		/// <summary>
		/// Creates new instance of <see cref="ConsoleListener">ConsoleListener</see> which doesn't listen any
		/// event type. 
		/// </summary>
		public ConsoleListener()
		{
			_acceptedEventTypes = new EventType[0];
		}

		/// <summary>
		/// Creates new instance of <see cref="ConsoleListener">ConsoleListener</see>.
		/// </summary>
		/// <param name="listenedEvents">Listened event types.</param>
		/// <remarks>See <see cref="Initialize">Initialize</see> for more detail.</remarks>		
		public ConsoleListener( bool[] listenedEvents )
		{
			Initialize( listenedEvents );
		}

		/// <summary>
		/// Initializes this listener to write events to console.
		/// </summary>
		/// <param name="listenedEvents"><c>bool</c> array describing which event types should be listened.</param>
		/// <remarks><c>listenedEvents</c> must have exactly
		/// <see cref="EventType.NUM_OF_EVENT_TYPES">EventType.NUM_OF_EVENT_TYPES</see> elements.
		/// See <see cref="EventType">EventType</see> for more detail.</remarks>
		public void Initialize( bool[] listenedEvents )
		{
			if( listenedEvents.Length != (int)EventType.NUM_OF_EVENT_TYPES )
				throw( new ArgumentException("Length of this array must be same as EventType.NUM_OF_EVENT_TYPES", "listenedEvents") );
						
			ArrayList acceptedEventTypes = new ArrayList();
			for( int i=0; i<listenedEvents.Length; i++ )
				if( listenedEvents[i] )
					acceptedEventTypes.Add( (EventType)i );
			_acceptedEventTypes = (EventType[])acceptedEventTypes.ToArray( typeof(EventType) );			
		}


		/// <summary>
		/// Get accepted event type.
		/// </summary>
		/// <param name="acceptedEventTypeIndex">Index of accepted event type.</param>
		/// <returns>Returns accepted <see cref="EventType">EventType</see>.</returns>
		/// <remarks>See <see cref="IListener.GetAcceptedEventType">IListener.GetAcceptedEventType</see>
		/// for more detail.</remarks>		
		public EventType GetAcceptedEventType(int acceptedEventTypeIndex)
		{
			return( _acceptedEventTypes[acceptedEventTypeIndex] );
		}

		/// <summary>
		/// Get accepted event type count.
		/// </summary>
		/// <returns>Returns number of accepted <see cref="EventType">EventType</see>.</returns>
		/// <remarks>See <see cref="IListener.GetAcceptedEventTypeCount">IListener.GetAcceptedEventTypeCount</see>
		/// for more detail.</remarks>
		public int GetAcceptedEventTypeCount()
		{
			return( _acceptedEventTypes.Length );
		}

		/// <summary>
		/// Writes one event to console.
		/// </summary>
		/// <param name="Event">Event to write.</param>
		/// <remarks>See <see cref="IListener.OnEvent">IListener.OnEvent</see>
		/// for more detail.</remarks>
		public void OnEvent(IEvent Event)
		{
			Console.WriteLine( Utils.EventToString(Event) );
		}
		
	}
	

	/// <summary>
	/// Simulation listener used to forward events to other listeners. 
	/// </summary>
	/// <remarks>
	/// <see cref="CompositionManager.Run">CompositionManager.Run</see> allows
	/// only one listener to monitor the simulation, if you need more than one listener,
	/// you can use this class. This class should be used only if simulation runs in same thread as UI,
	/// in other case use <see cref="ProxyMultiThreadListener">ProxyMultiThreadListener</see>.	
	/// </remarks>	
	public class ProxyListener: IListener
	{		
		InternalListenerRecord[] _internalListeners;
		EventType[] _acceptedEventTypes;				
		
		private struct InternalListenerRecord 
		{			
			public bool[] listenedEventTypes;
			public IListener listener;
		}

		/// <summary>
		/// Initializes this <see cref="ProxyListener">ProxyListener</see> to send events to specific listeners.
		/// </summary>
		/// <param name="listeners">Listeners to recieve events.</param>
		/// <remarks>
		/// All registered listeners may not change content of the event
		/// in theit implementation of <see cref="IListener.OnEvent">IListener.OnEvent</see> method.
		/// See <see cref="OnEvent">OnEvent</see>, <see cref="ProxyListener">ProxyListener</see>
		/// for more detail.
		/// </remarks>	
		public void Initialize( ArrayList listeners )
		{
			_internalListeners = new InternalListenerRecord[ listeners.Count ];

			bool[] listenedEventTypes = new bool[ (int)EventType.NUM_OF_EVENT_TYPES ];
			for( int i=0; i<listenedEventTypes.Length; i++ )
				listenedEventTypes[i] = false;
			
			// create internal table of listeners and set their listened events
			for( int i=0; i<listeners.Count; i++ )
			{
				IListener listener = (IListener)listeners[i];
				_internalListeners[i].listener = listener;
				_internalListeners[i].listenedEventTypes = new bool[ (int)EventType.NUM_OF_EVENT_TYPES ];

				for( int j=0; j<(int)EventType.NUM_OF_EVENT_TYPES; j++ )
					_internalListeners[i].listenedEventTypes[j] = false;

				for( int j=0; j<listener.GetAcceptedEventTypeCount(); j++ )
				{
					_internalListeners[i].listenedEventTypes[ (int)listener.GetAcceptedEventType(j) ] = true;
					listenedEventTypes[ (int)listener.GetAcceptedEventType(j) ] = true;
				}
			}

			// set this listener's accepted event types
			ArrayList acceptedEventTypes = new ArrayList();
			for( int i=0; i<listenedEventTypes.Length; i++ )
				if( listenedEventTypes[i] )
					acceptedEventTypes.Add( (EventType)i );
			_acceptedEventTypes = (EventType[])acceptedEventTypes.ToArray( typeof(EventType) );	

		}

		/// <summary>
		/// Get accepted event type.
		/// </summary>
		/// <param name="acceptedEventTypeIndex">Index of accepted event type.</param>
		/// <returns>Returns accepted <see cref="EventType">EventType</see>.</returns>
		/// <remarks>See <see cref="IListener.GetAcceptedEventType">IListener.GetAcceptedEventType</see>
		/// for more detail.</remarks>		
		public EventType GetAcceptedEventType(int acceptedEventTypeIndex)
		{			
			return( _acceptedEventTypes[acceptedEventTypeIndex] );
		}

		/// <summary>
		/// Get accepted event type count.
		/// </summary>
		/// <returns>Returns number of accepted <see cref="EventType">EventType</see>.</returns>
		/// <remarks>See <see cref="IListener.GetAcceptedEventTypeCount">IListener.GetAcceptedEventTypeCount</see>
		/// for more detail.</remarks>
		public int GetAcceptedEventTypeCount()
		{
			return( _acceptedEventTypes.Length );
		}

		/// <summary>
		/// Sends this event to all registered listeners.
		/// </summary>
		/// <param name="Event">Event to send.</param>
		/// <remarks>See <see cref="IListener.OnEvent">IListener.OnEvent</see>
		/// for more detail.</remarks>		
		public void OnEvent( IEvent Event )
		{
			foreach( InternalListenerRecord record in _internalListeners )
				if( record.listenedEventTypes[(int)Event.Type] 
					|| Event==CompositionManager.SimulationFinishedEvent
					|| Event==CompositionManager.SimulationFailedEvent )
					record.listener.OnEvent( Event );
		}		
	
	}
	

	/// <summary>
	/// Simulation listener used to forward events to other listeners. 
	/// Moreover, compared to <see cref="ProxyListener">ProxyListener</see>, 
	/// it's able to synchronize access to events and resources influed by them
	/// between simulation thread and UI thread.
	/// </summary>
	/// <remarks>
	/// This class should be used only if simulation runs in other thread than registered listeners should and if
	/// application has a message loop (see <see cref="Application.MessageLoop">Application.MessageLoop</see>).
	/// During simulation, if some model generates an event, the simulation thread invokes 
	/// <see cref="OnEvent">OnEvent</see> method. This method stores events into internal queues.
	/// In some interval, timer defined by the user (see <see cref="Initialize">Initialize</see> method)
	/// generates in UI thread an timer-event which processes all events from
	/// internal queues using <see cref="SendEventsToListeners">SendEventsToListeners</see>
	/// method.
	/// The goal is that <see cref="OnEvent">OnEvent</see> method is called from simulation thread,
	/// and <see cref="SendEventsToListeners">SendEventsToListeners</see> from UI thread
	/// (ie. the thread with message loop) and access to internal list of events is synchronized
	/// between them by mutex.
	/// The next goal is that with this mechanism we don't have to use active waiting
	/// to determine when simulation finished.
	/// If simulation thread would directly call UI-side listeners
	/// (for example <see cref="ProgressBarListener">ProgressBarListener</see>), the result 
	/// will depend on race condition (for example progress-bar could be increased while repainting it).
	/// The only issue is that we silently expect that listeners won't change content of the event.
	/// </remarks>
	/*public class ProxyMultiThreadListener: IListener
	{
		EventType[] _acceptedEventTypes;
		Mutex _mutex;		
		InternalListenerRecord[] _internalListeners;
		//System.Windows.Forms.Timer _timer;
		
		/// <summary>
		/// Internal record which is hold for each listener registered
		/// with <see cref="Initialize">Initialize</see> method.
		/// </summary>
		private struct InternalListenerRecord 
		{
			public Queue eventQueue;
			public bool[] listenedEventTypes;
			public IListener listener;
		}


		/// <summary>
		/// Class used to store basic content of any class inherited from <see cref="IEvent">IEvent</see>.
		/// </summary>
		/// <remarks>
		/// This class is used in <see cref="OnEvent">OnEvent</see>
		/// to copy content of received event, because sender can change
		/// it's content before it's delivered to registered listeners.
		/// The usage of backbone's <see cref="Oatc.OpenMI.Sdk.Backbone.Event">Event</see>
		/// isn't effective here because it creates new <c>Hashtable</c> everytime it's created.
		/// </remarks>
		private class EventCopy: IEvent
		{
			private string _description;
			private EventType _type;
			private ILinkableComponent _sender;
			private TimeStamp _simulationTime;
 
			public object GetAttribute(string key)
			{			
				return null;
			}

			public string Description
			{
				get	{ return( _description ); }
			}

			public EventType Type
			{
				get	{ return( _type ); }
			}

			public ILinkableComponent Sender
			{
				get	{ return( _sender ); }
			}

			public ITimeStamp SimulationTime
			{
				get	{ return( _simulationTime ); }
			}

			public EventCopy( IEvent theEvent )
			{
				_description = theEvent.Description;
				_type = theEvent.Type;
				_sender = theEvent.Sender;
				if( theEvent.SimulationTime == null )
					_simulationTime = null;
				else
					_simulationTime = new TimeStamp( theEvent.SimulationTime );
			}
		}

	
		/// <summary>
		/// Initializes this instance to send events to specific listeners.
		/// </summary>
		/// <param name="listeners">Listeners to recieve events.</param>
		/// <param name="timer">Timer used to fire sending of events to registered listeners.</param>
		/// <param name="timerInterval">Time interval in milliseconds between sending of events to registered listeners.</param>
		/// <remarks>
		/// All registered listeners may not change content of the event
		/// in their implementation of <see cref="IListener.OnEvent">IListener.OnEvent</see> method.		
		/// See <see cref="OnEvent">OnEvent</see>, <see cref="ProxyMultiThreadListener">ProxyMultiThreadListener</see>
		/// for more detail.
		/// </remarks>		
		public void Initialize( ArrayList listeners, System.Windows.Forms.Timer timer, int timerInterval )
		{	
			_internalListeners = new InternalListenerRecord[ listeners.Count ];
			
			bool[] listenedEventTypes = new bool[ (int)EventType.NUM_OF_EVENT_TYPES ];
			for( int i=0; i<listenedEventTypes.Length; i++ )
				listenedEventTypes[i] = false;

			// create internal table of listeners and set their listened events
			for( int i=0; i<listeners.Count; i++ )
			{
				IListener listener = (IListener)listeners[i];
				_internalListeners[i].eventQueue = new Queue();
				_internalListeners[i].listener = listener;
				_internalListeners[i].listenedEventTypes = new bool[ (int)EventType.NUM_OF_EVENT_TYPES ];

				for( int j=0; j<(int)EventType.NUM_OF_EVENT_TYPES; j++ )
					_internalListeners[i].listenedEventTypes[j] = false;

				for( int j=0; j<listener.GetAcceptedEventTypeCount(); j++ )
				{
					_internalListeners[i].listenedEventTypes[ (int)listener.GetAcceptedEventType(j) ] = true;
					listenedEventTypes[ (int)listener.GetAcceptedEventType(j) ] = true;
				}
			}

			// set this listener's accepted event types
			ArrayList acceptedEventTypes = new ArrayList();
			for( int i=0; i<listenedEventTypes.Length; i++ )
				if( listenedEventTypes[i] )
					acceptedEventTypes.Add( (EventType)i );
			_acceptedEventTypes = (EventType[])acceptedEventTypes.ToArray( typeof(EventType) );	
			
			_mutex = new Mutex();

			Debug.Assert( !timer.Enabled );

			_timer = timer;			
			_timer.Interval = timerInterval;
			_timer.Tick += new System.EventHandler( SendEventsToListenersHandler );
			_timer.Enabled = true;
		}

	

		/// <summary>
		/// Get accepted event type.
		/// </summary>
		/// <param name="acceptedEventTypeIndex">Index of accepted event type.</param>
		/// <returns>Returns accepted <see cref="EventType">EventType</see>.</returns>
		/// <remarks><see cref="ProxyMultiThreadListener">ProxyMultiThreadListener</see>
		/// accepts exactly the event types accepted by one of listeners
		/// registered with <see cref="Initialize">Initialize</see> method.
		/// See <see cref="IListener.GetAcceptedEventType">IListener.GetAcceptedEventType</see>
		/// for more detail.</remarks>
		public EventType GetAcceptedEventType(int acceptedEventTypeIndex)
		{			
			return( (EventType)_acceptedEventTypes[acceptedEventTypeIndex] );
		}


		/// <summary>
		/// Get accepted event type count.
		/// </summary>
		/// <returns>Returns number of accepted <see cref="EventType">EventType</see>.</returns>
		/// <remarks>See <see cref="IListener.GetAcceptedEventTypeCount">IListener.GetAcceptedEventTypeCount</see>
		/// for more detail.</remarks>
		public int GetAcceptedEventTypeCount()
		{
			return( _acceptedEventTypes.Length );
		}


		/// <summary>
		/// Method stores copy of the event to internal queues so it later can
		/// be send to registered listeners.
		/// </summary>
		/// <param name="Event">Event to be queued.</param>
		/// <remarks>This method is called from thread where events are created (ie. simulation thread).
		/// We must make copy of the event for case caller changes it before it's delivered
		/// to registered listeners.
		/// Call blocks until event can be stored to internal queues.
		/// See <see cref="ProxyMultiThreadListener">ProxyMultiThreadListener</see>,
		/// <see cref="Initialize">Initialize</see> for more detail.
		/// </remarks>
		public void OnEvent( IEvent Event )
		{			
			_mutex.WaitOne();

			IEvent eventCopy;
			bool finished = false;

			// if event has special meaning, we don't have to copy it
			// because in such case we couldn't later determine it's meaning
			if( Event==CompositionManager.SimulationFinishedEvent
				|| Event==CompositionManager.SimulationFailedEvent )
			{
				eventCopy = Event;
				finished = true;
			}
			else
				eventCopy = new EventCopy( Event );			

			// add this event to registered listener queues
			foreach( InternalListenerRecord record in _internalListeners )
				if( record.listenedEventTypes[(int)eventCopy.Type] 
					|| finished )
					record.eventQueue.Enqueue( eventCopy );

			_mutex.ReleaseMutex();			
		}

		

		/// <summary>
		/// This function sends all queued events to listeners registered using
		/// <see cref="Initialize">Initialize</see> method.
		/// </summary>
		/// <remarks>
		/// Events are queued by simulation thread using <see cref="OnEvent">OnEvent</see> method.
		/// Call this function from thread, where you want registered listeners to work (ie. from UI thread).
		/// Calling thread blocks until all registered listeners have done its work.
		/// If simulation finished, the timer is disabled to stop calling it's handler.
		/// </remarks>
		public bool SendEventsToListeners()
		{
			bool finished = false;

			_mutex.WaitOne();

			try 
			{			
				foreach( InternalListenerRecord record in _internalListeners )
				{					
					ListViewListener listViewListener = record.listener as ListViewListener;
					if( listViewListener!=null )
					{
						// optimization for case listener is ListBoxListener
						finished |= listViewListener.AddQueuedEvents(record.eventQueue);
					}
					else
					{
						// send all events separatelly
						while( record.eventQueue.Count > 0 )
						{
							IEvent theEvent = (IEvent)record.eventQueue.Dequeue();
						
							finished |= theEvent==CompositionManager.SimulationFinishedEvent || theEvent==CompositionManager.SimulationFailedEvent;								

							record.listener.OnEvent( theEvent );
						}
					}
					
				}
			}
			finally
			{
				_mutex.ReleaseMutex();
			}

			if( finished )			
				if( _timer.Enabled )
				{
					_timer.Enabled = false;
					_timer.Tick -= new System.EventHandler( SendEventsToListenersHandler );
				}			

			return( finished );
		}
	

		/// <summary>
		/// Handler used by timer.
		/// </summary>
		/// <param name="sender">Sender, not used.</param>
		/// <param name="e">Event arguments, not used.</param>
		private void SendEventsToListenersHandler( object sender, EventArgs e )
		{
			SendEventsToListeners();
		}

	}*/
}
