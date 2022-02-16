#include <CoreFoundation/CoreFoundation.h>
#include <pthread.h>
#include <mach/mach_time.h>
#include <mach/mach.h>

typedef struct
{
    pthread_t thread;
    char active;
	CFRunLoopRef runLoopRef;
} TimerSessionHandle;

typedef struct
{
    void (*callback)(void);
    CFRunLoopTimerRef timerRef;
} TimerInfo;

void EmptyCallback(CFRunLoopTimerRef timer, void *info)
{
}

void* TimerSessionThreadRoutine(void* data)
{
    TimerSessionHandle* sessionHandle = (TimerSessionHandle*)data;

    CFRunLoopTimerContext context = { 0, NULL, NULL, NULL, NULL };
	CFRunLoopTimerRef timerRef = CFRunLoopTimerCreate(
	    NULL,
		CFAbsoluteTimeGetCurrent() + 60,
		60,
		0,
		0,
		EmptyCallback,
		&context);

    CFRunLoopRef runLoopRef = CFRunLoopGetCurrent();
	CFRunLoopAddTimer(runLoopRef, timerRef, kCFRunLoopDefaultMode);
	
	// Set realtime priority
    // (thanks to https://stackoverflow.com/a/44310370/2975589)

    mach_timebase_info_data_t timebase;
    mach_timebase_info(&timebase);

    struct thread_time_constraint_policy constraintPolicy;

    constraintPolicy.period = 500 * 1000 * timebase.denom / timebase.numer; // Period over which we demand scheduling.
    constraintPolicy.computation = 100 * 1000 * timebase.denom / timebase.numer; // Minimum time in a period where we must be running.
    constraintPolicy.constraint = 100 * 1000 * timebase.denom / timebase.numer; // Maximum time between start and end of our computation in the period.
    constraintPolicy.preemptible = FALSE;

    thread_port_t threadId = pthread_mach_thread_np(pthread_self());
    thread_policy_set(threadId, THREAD_TIME_CONSTRAINT_POLICY, (thread_policy_t)&constraintPolicy, THREAD_TIME_CONSTRAINT_POLICY_COUNT);

    //

    sessionHandle->active = 1;
	sessionHandle->runLoopRef = runLoopRef;

    CFRunLoopRun();

    return NULL;
}

void OpenTimerSession(void** handle)
{
	TimerSessionHandle* sessionHandle = malloc(sizeof(TimerSessionHandle));

	sessionHandle->active = 0;
    pthread_create(&sessionHandle->thread, NULL, TimerSessionThreadRoutine, sessionHandle);
    while (sessionHandle->active == 0) { }
	
    *handle = sessionHandle;
}

void TimerCallback(CFRunLoopTimerRef timer, void *info)
{
	TimerInfo* timerInfo = (TimerInfo*)info;
	timerInfo->callback();
}

void StartTimer(int interval, TimerSessionHandle* sessionHandle, void (*callback)(void), TimerInfo** info)
{
    TimerInfo* timerInfo = malloc(sizeof(TimerInfo));
    timerInfo->callback = callback;
	
	double seconds = (double)interval / 1000.0;
	
	CFRunLoopTimerContext context = { 0, timerInfo, NULL, NULL, NULL };
	CFRunLoopTimerRef timerRef = CFRunLoopTimerCreate(
	    NULL,
		CFAbsoluteTimeGetCurrent() + seconds,
		seconds,
		0,
		0,
		TimerCallback,
		&context);

    timerInfo->timerRef = timerRef;
	CFRunLoopAddTimer(sessionHandle->runLoopRef, timerRef, kCFRunLoopDefaultMode);

    *info = timerInfo;
}

void StopTimer(TimerSessionHandle* sessionHandle, TimerInfo* timerInfo)
{
    CFRunLoopRemoveTimer(sessionHandle->runLoopRef, timerInfo->timerRef, kCFRunLoopDefaultMode);
}