import os

pairs = sorted((
    ((name := f"{dr}/{f}"), os.path.getsize(name))
    for dr, _, fs in os.walk(".")
    for f in fs
), key=lambda p: p[1])

print(*pairs, sep='\n')


