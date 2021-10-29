MONSTER_KILLED_EVENT = "monster_killed"


subscribers = dict()


def subscribe(event_type: str, fn):
    if event_type not in subscribers:
        subscribers[event_type] = []
    subscribers[event_type].append(fn)


async def post_event(event_type: str, data):
    if event_type not in subscribers:
        return
    for fn in subscribers[event_type]:
        await fn(data)
