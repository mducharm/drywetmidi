#include <CoreFoundation/CoreFoundation.h>
#include <pthread.h>
#include <time.h>
#include <mach/mach_time.h>
#include <mach/mach.h>

typedef struct
{
    pthread_t thread;
    char active;
    int intervalMs;
    void (*callback)(void);
} TimerInfo;

void SetRealtimePriority()
{
    mach_timebase_info_data_t timebase;
    mach_timebase_info(&timebase);

    struct thread_time_constraint_policy constraintPolicy;

    constraintPolicy.period = 500 * 1000 * timebase.denom / timebase.numer; // Period over which we demand scheduling.
    constraintPolicy.computation = 100 * 1000 * timebase.denom / timebase.numer; // Minimum time in a period where we must be running.
    constraintPolicy.constraint = 100 * 1000 * timebase.denom / timebase.numer; // Maximum time between start and end of our computation in the period.
    constraintPolicy.preemptible = FALSE;

    thread_port_t threadId = pthread_mach_thread_np(pthread_self());
    thread_policy_set(threadId, THREAD_TIME_CONSTRAINT_POLICY, (thread_policy_t)&constraintPolicy, THREAD_TIME_CONSTRAINT_POLICY_COUNT);
}

void* TimerThreadRoutine(void* data)
{
    TimerInfo* timerInfo = (TimerInfo*)data;
	
	SetRealtimePriority();
    timerInfo->active = 1;

    while (timerInfo->active == 1)
    {
        struct timespec req;
        req.tv_sec = 0;
        req.tv_nsec = timerInfo->intervalMs * 1000000;
		nanosleep(&req, NULL);
        
        timerInfo->callback();
    }

    return NULL;
}

void StartTimer(int intervalMs, void (*callback)(void), TimerInfo** info)
{
    TimerInfo* timerInfo = malloc(sizeof(TimerInfo));
    timerInfo->callback = callback;
    timerInfo->intervalMs = intervalMs;
	
    timerInfo->active = 0;
    pthread_create(&timerInfo->thread, NULL, TimerThreadRoutine, timerInfo);
    while (timerInfo->active == 0) { }
	
    *info = timerInfo;
}

void StopTimer(TimerInfo* timerInfo)
{
    timerInfo->active = 0;
}