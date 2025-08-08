from dataclasses import dataclass
import math
import random

@dataclass
class item:
    start: int
    end: int


def FindFirstBefore(source: list[item], offset: int, counts: list[int]):
    result = -1

    left = 0
    right = len(source) - 1
    count = 0
    while left <= right:
        mid = left + (right - left) // 2;
        count += 1
        current = source[mid]
        if (current.end < offset):
            result = mid;
            left = mid + 1;
            continue
        right = mid - 1
    counts.append(count)
    return result

def FindFirstAfter(source: list[item], offset: int, counts: list[int]):
    result = -1

    left = 0
    right = len(source) - 1
    count = 0
    while left <= right:
        mid = left + (right - left) // 2;
        count += 1
        current = source[mid]
        if (current.start > offset):
            result = mid;
            right = mid - 1
            continue
        left = mid + 1;
    counts.append(count)
    return result

def FindLastNotAfter(source: list[item], offset: int, counts: list[int]):
    result = -1
    left = 0
    right = len(source) - 1
    count = 0
    while left <= right:
        mid = left + (right - left) // 2;
        count += 1
        current = source[mid]
        if (current.start < offset):
            result = mid;
            left = mid + 1;
            continue
        right = mid - 1
    counts.append(count)
    return result

all_items = [item(a, a + random.randint(1, 15)) for a in range(0, 10000, 15)]

for kk in range(5):
    items = all_items[:1 << kk]

    counts = []
    for i in range(items[-1].end + 5):
        # x = FindFirstBefore(items, i, counts)
        # lsx = [it for it in items if it.end < i]
        # assert (len(lsx) == 0 and x == -1) \
        #         or (len(lsx) > 0 and items[x] == lsx[-1] and items[x].end < i), \
        #     f"{i}, {x}, {lsx}"

        # y = FindFirstAfter(items, i, counts)
        # lsy = [it for it in items if it.start > i]
        # assert (len(lsy) == 0 and y == -1) \
        #         or (len(lsy) > 0 and items[y] == lsy[0] and items[y].start > i), \
        #     f"{i}, {y}"

        z = FindLastNotAfter(items, i, counts)
        lsz = [it for it in items if it.start < i]
        assert (len(lsz) == 0 and z == -1) \
            or (len(lsz) > 0 and items[z] == lsz[-1] and items[z].start < i), \
            f"{i}, {z}"

    avg = sum(counts) / len(counts)
    # print(avg, len(items))

    print((avg - kk) * (2 ** kk))