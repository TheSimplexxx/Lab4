using System;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Collections.Generic;


class CustomEventBus
{
    
    private Dictionary<Type, List<Delegate>> eventHandlers = new Dictionary<Type, List<Delegate>>();

    
    private Dictionary<Type, DateTime> lastEventTimes = new Dictionary<Type, DateTime>();

    
    private int throttleInterval;

    public CustomEventBus(int throttleInterval)
    {
        this.throttleInterval = throttleInterval;
    }

    
    public void RegisterHandler<TEvent>(Action<TEvent> handler)
    {
        Type eventType = typeof(TEvent);

        if (!eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType] = new List<Delegate>();
            lastEventTimes[eventType] = DateTime.MinValue;
        }

        eventHandlers[eventType].Add(handler);
    }

    
    public void UnregisterHandler<TEvent>(Action<TEvent> handler)
    {
        Type eventType = typeof(TEvent);

        if (eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType].Remove(handler);
        }
    }

  
    public void Dispatch<TEvent>(TEvent ev)
    {
        Type eventType = typeof(TEvent);

        
        DateTime lastEventTime = lastEventTimes[eventType];
        DateTime currentTime = DateTime.Now;
        TimeSpan elapsedTime = currentTime - lastEventTime;
        if (elapsedTime.TotalMilliseconds < throttleInterval)
        {
          
            return;
        }
        lastEventTimes[eventType] = currentTime;

        if (eventHandlers.ContainsKey(eventType))
        {
          
            foreach (Delegate handler in eventHandlers[eventType])
            {
                if (handler is Action<TEvent> typedHandler)
                {
                    typedHandler(ev);
                }
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
       
        CustomEventBus eventBus = new CustomEventBus(1000);

        
        eventBus.RegisterHandler<string>(s => Console.WriteLine("Received string event: " + s));
        eventBus.RegisterHandler<int>(i => Console.WriteLine("Received int event: " + i));

       
        eventBus.Dispatch("hello");
        eventBus.Dispatch(42);
        eventBus.Dispatch("world");
        eventBus.Dispatch(123);

       
        Thread.Sleep(3000);

        
        eventBus.Dispatch("foo");
        eventBus.Dispatch(99);

      
        eventBus.UnregisterHandler<int>(i => Console.WriteLine("Received int event: " + i));
    }
}

public class Event
{
    public int Priority { get; set; }
    public string Data { get; set; }
}

public class PriorityQueue
{
    private SortedDictionary<int, Queue<Event>> _events = new SortedDictionary<int, Queue<Event>>();

    public void Enqueue(Event e)
    {
        if (!_events.ContainsKey(e.Priority))
        {
            _events.Add(e.Priority, new Queue<Event>());
        }
        _events[e.Priority].Enqueue(e);
    }

    public Event Dequeue()
    {
        if (_events.Count == 0) return null;
        var highestPriority = _events.Keys.Max();
        var eventQueue = _events[highestPriority];
        var e = eventQueue.Dequeue();
        if (eventQueue.Count == 0) _events.Remove(highestPriority);
        return e;
    }
}

public class Subscriber
{
    private PriorityQueue _queue;
    private Action<Event> _handler;

    public Subscriber(PriorityQueue queue, int priority, Action<Event> handler)
    {
        _queue = queue;
        _handler = handler;
        _queue.Enqueue(new Event { Priority = priority, Data = null });
    }

    public void ReceiveEvent()
    {
        var e = _queue.Dequeue();
        if (e != null) _handler(e);
    }
}

public class Publisher
{
    private Action<Event> _publishDelegate;
    private PriorityQueue _queue = new PriorityQueue();

    public Publisher(Action<Event> publishDelegate)
    {
        _publishDelegate = publishDelegate;
    }

    public void Publish(int priority, string data)
    {
        var e = new Event { Priority = priority, Data = data };
        _queue.Enqueue(e);
        _publishDelegate(e);
    }
}

public class Program3
{
    static void Main(string[] args)
    {
        var queue = new PriorityQueue();
        var subscriber1 = new Subscriber(queue, 1, e => Console.WriteLine($"Received priority {e.Priority} event: {e.Data}"));
        var subscriber2 = new Subscriber(queue, 2, e => Console.WriteLine($"Received priority {e.Priority} event: {e.Data}"));
        var subscriber3 = new Subscriber(queue, 3, e => Console.WriteLine($"Received priority {e.Priority} event: {e.Data}"));

        var publisher = new Publisher(e => queue.Enqueue(e));
        publisher.Publish(1, "hello");
        publisher.Publish(2, "world");
        publisher.Publish(1, "foo");
        publisher.Publish(3, "bar");
        publisher.Publish(2, "baz");

        subscriber1.ReceiveEvent();
        subscriber2.ReceiveEvent();
        subscriber3.ReceiveEvent();
        subscriber1.ReceiveEvent();
        subscriber2.ReceiveEvent();
        subscriber3.ReceiveEvent();
    }
}