using System;
using System.Collections.Generic;
using System.Text;

// $Id$

namespace Pango
{
    // ----- class Schedule ----------------------------------------
    public class Schedule
    {
        public delegate void EventDelegate();

        // ----- fields --------------------

        private int time;
        // priority queue
        SortedList<int, Queue<EventDelegate>> pqueue;

        // ----- constructors --------------------

        public Schedule() {
            time = 0;
            pqueue = new SortedList<int, Queue<EventDelegate>>();
        }

        // ----- properties --------------------

        public int Time {
            get { return time; }
        }

        // ----- methods --------------------

        public void add(EventDelegate eh, int timeoffset) {
            if (pqueue == null) { return; }
            int priority = time + timeoffset;
            if (!pqueue.ContainsKey(priority)) {
                pqueue.Add(priority, new Queue<EventDelegate>());
            }
            pqueue[priority].Enqueue(eh);
        }

        public void callCurrentEvents() {
            if (pqueue == null) { return; }
            if (pqueue.ContainsKey(time)) {
                EventDelegate action;
                Queue<EventDelegate> queue = pqueue[time];
                while (queue.Count > 0) {
                    action = queue.Dequeue();
                    action(); // perform the scheduled action
                }
                pqueue.Remove(time);
            }
        }

        public void increaseTime() {
            time++;
        }

        public bool empty() {
            if (pqueue != null) {
                return (pqueue.Count <= 0);
            } else {
                return true;
            }
        }

        public void clear() {
            if (pqueue != null) { pqueue.Clear(); }
            time = 0;
        }
    }
}
