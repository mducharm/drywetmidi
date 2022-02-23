#include <CoreFoundation/CoreFoundation.h>
#include <pthread.h>
#include <time.h>

typedef struct
{
    pthread_t thread;
    char active;
    int intervalMs;
    void (*callback)(void);
} TimerInfo;

void* TimerThreadRoutine(void* data)
{
    TimerInfo* timerInfo = (TimerInfo*)data;
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