// -----------------------------------------------------------------------
// <copyright file = "EventSourceBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Tracing.EventDefinitions
{
    using System;
    using System.Diagnostics.Tracing;

    /// <summary>
    /// Utility event source to provide common methods to be used by all event sources,
    /// most likely some optimized overloads of WriteEvent method.
    /// </summary>
    /// <remarks>
    /// In this base class we didn't overload the method WriteEvent due to the method overloading distance problem
    /// described here https://ericlippert.com/2013/12/23/closer-is-better/.
    /// Instead, we name the methods separately following the rule of
    /// WriteEvent[arg1Type][x][occurrences][arg2Type][x][occurrences]...[argXType][x][occurrences];
    /// For example, if a event emits the payload of (int, bool, bool), we will name it WriteEventIntBoolx2,
    /// while the method signature will be WriteEventIntBool(int, int, bool, bool) since the first parameter will
    /// always be the event id.
    /// </remarks>
    public abstract class EventSourceBase : EventSource
    {
        /// <summary>
        /// Writes an event by using the provided event identifier and payloads which are ordered in (int, bool).
        /// </summary>
        /// <param name="eventId">The event identifier.  This value should be between 0 and 65535.</param>
        /// <param name="arg1">A 32 bit integer argument.</param>
        /// <param name="arg2">A 8 bit boolean argument.</param>
        [NonEvent]
        protected unsafe void WriteEventIntBool(int eventId, int arg1, bool arg2)
        {
            if(!IsEnabled())
            {
                return;
            }

            var eventData = stackalloc EventData[2];

            eventData[0].DataPointer = (IntPtr)(&arg1);
            eventData[0].Size = sizeof(int);

            eventData[1].DataPointer = (IntPtr)(&arg2);
            eventData[1].Size = sizeof(bool);

            WriteEventCore(eventId, 2, eventData);
        }
        
        /// <summary>
        /// Writes an event by using the provided event identifier and a bool payload.
        /// </summary>
        /// <param name="eventId">The event identifier.  This value should be between 0 and 65535.</param>
        /// <param name="arg1">A 8 bit boolean argument.</param>
        [NonEvent]
        protected unsafe void WriteEventBool(int eventId, bool arg1)
        {
            if(!IsEnabled())
            {
                return;
            }

            var eventData = stackalloc EventData[1];

            eventData[0].DataPointer = (IntPtr)(&arg1);
            eventData[0].Size = sizeof(bool);
            
            WriteEventCore(eventId, 1, eventData);
        }

        /// <summary>
        /// Writes an event by using the provided event identifier and payloads which are ordered in (int, long).
        /// </summary>
        /// <param name="eventId">The event identifier.  This value should be between 0 and 65535.</param>
        /// <param name="arg1">A 32 bit integer argument.</param>
        /// <param name="arg2">A 64 bit integer argument.</param>
        [NonEvent]
        protected unsafe void WriteEventIntLong(int eventId, int arg1, long arg2)
        {
            if(!IsEnabled())
            {
                return;
            }

            var eventData = stackalloc EventData[2];

            eventData[0].DataPointer = (IntPtr)(&arg1);
            eventData[0].Size = sizeof(int);

            eventData[1].DataPointer = (IntPtr)(&arg2);
            eventData[1].Size = sizeof(long);

            WriteEventCore(eventId, 2, eventData);
        }

        /// <summary>
        /// Writes an event by using the provided event identifier and payloads which are ordered in
        /// (int, int, int, int).
        /// </summary>
        /// <param name="eventId">The event identifier.  This value should be between 0 and 65535.</param>
        /// <param name="arg1">A 32 bit integer argument.</param>
        /// <param name="arg2">A 32 bit integer argument.</param>
        /// <param name="arg3">A 32 bit integer argument.</param>
        /// <param name="arg4">A 32 bit integer argument.</param>
        [NonEvent]
        protected unsafe void WriteEventIntx4(int eventId, int arg1, int arg2, int arg3, int arg4)
        {
            if(!IsEnabled())
            {
                return;
            }

            var eventData = stackalloc EventData[4];
            
            eventData[0].DataPointer = (IntPtr)(&arg1);
            eventData[0].Size = sizeof(int);
            
            eventData[1].DataPointer = (IntPtr)(&arg2);
            eventData[1].Size = sizeof(int);
            
            eventData[2].DataPointer = (IntPtr)(&arg3);
            eventData[2].Size = sizeof(int);
            
            eventData[3].DataPointer = (IntPtr)(&arg4);
            eventData[3].Size = sizeof(int);

            WriteEventCore(eventId, 4, eventData);
        }

        /// <summary>
        /// Writes an event by using the provided event identifier and payloads which are ordered in
        /// (int, int, int, int, int).
        /// </summary>
        /// <param name="eventId">The event identifier.  This value should be between 0 and 65535.</param>
        /// <param name="arg1">A 32 bit integer argument.</param>
        /// <param name="arg2">A 32 bit integer argument.</param>
        /// <param name="arg3">A 32 bit integer argument.</param>
        /// <param name="arg4">A 32 bit integer argument.</param>
        /// <param name="arg5">A 32 bit integer argument.</param>
        [NonEvent]
        protected unsafe void WriteEventIntx5(int eventId, int arg1, int arg2, int arg3, int arg4, int arg5)
        {
            if(!IsEnabled())
            {
                return;
            }

            var eventData = stackalloc EventData[5];
            
            eventData[0].DataPointer = (IntPtr)(&arg1);
            eventData[0].Size = sizeof(int);
            
            eventData[1].DataPointer = (IntPtr)(&arg2);
            eventData[1].Size = sizeof(int);
            
            eventData[2].DataPointer = (IntPtr)(&arg3);
            eventData[2].Size = sizeof(int);
            
            eventData[3].DataPointer = (IntPtr)(&arg4);
            eventData[3].Size = sizeof(int);
            
            eventData[4].DataPointer = (IntPtr)(&arg5);
            eventData[4].Size = sizeof(int);
            
            WriteEventCore(eventId, 5, eventData);
        }
        
        /// <summary>
        /// Writes an event by using the provided event identifier and payloads which are ordered in
        /// (string, string, int).
        /// </summary>
        /// <param name="eventId">The event identifier.  This value should be between 0 and 65535.</param>
        /// <param name="arg1">A string argument.</param>
        /// <param name="arg2">A string argument.</param>
        /// <param name="arg3">A 32 bit integer argument.</param>
        [NonEvent]
        protected unsafe void WriteEventStringx2Int(int eventId, string arg1, string arg2, int arg3)
        {
            if(!IsEnabled())
            {
                return;
            }

            if(arg1 == null)
            {
                arg1 = string.Empty;
            }

            if(arg2 == null)
            {
                arg2 = string.Empty;
            }
            
            fixed(char* charPtr1 = arg1)
            fixed(char* charPtr2 = arg2)
            {
                var eventData = stackalloc EventData[3];
            
                eventData[0].DataPointer = (IntPtr)charPtr1;
                // null terminated string has one extra '\0' at the end.
                eventData[0].Size = (arg1.Length + 1) * sizeof(char);
            
                eventData[1].DataPointer = (IntPtr)charPtr2;
                eventData[1].Size = (arg2.Length + 1) * sizeof(char);
            
                eventData[2].DataPointer = (IntPtr)(&arg3);
                eventData[2].Size = sizeof(int);

                WriteEventCore(eventId, 3, eventData);
            }
        }
    }
}