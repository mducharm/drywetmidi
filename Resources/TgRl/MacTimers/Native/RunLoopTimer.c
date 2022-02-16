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

void StartTimer(int intervalMs, TimerSessionHandle* sessionHandle, void (*callback)(void), TimerInfo** info)
{
    TimerInfo* timerInfo = malloc(sizeof(TimerInfo));
    timerInfo->callback = callback;
	
	double seconds = (double)intervalMs / 1000.0;
	
	CFRunLoopTimerContext context = { 0, timerInfo, NULL, NULL, NULL };
	timerInfo->timerRef = CFRunLoopTimerCreate(
	    NULL,
		CFAbsoluteTimeGetCurrent() + seconds,
		seconds,
		0,
		0,
		TimerCallback,
		&context);
	CFRunLoopAddTimer(sessionHandle->runLoopRef, timerRef, kCFRunLoopDefaultMode);
	
    *info = timerInfo;
}

void StopTimer(TimerSessionHandle* sessionHandle, TimerInfo* timerInfo)
{
    CFRunLoopRemoveTimer(sessionHandle->runLoopRef, timerInfo->timerRef, kCFRunLoopDefaultMode);
}