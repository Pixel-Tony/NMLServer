# import itertools
# import os
from typing import Iterable
import nml.editors.extract_tables as extracted

def write(fname: str, content: Iterable[str]):
    with open(fname, "w", encoding="utf8") as file:
        file.write("\n".join(content))

write("./constants.txt", sorted(extracted.constant_names))

write("./features.txt", extracted.feature_names_table)

write("./properties.txt", sorted(
    extracted.properties | extracted.callbacks | extracted.layout_sprites
    | extracted.act14_vars))

write("./variables.txt", sorted(extracted.variables))

# files = [f for f in os.listdir(".") if ".py" not in f]
# pairs = itertools.combinations(files, 2)
# for a, b in pairs:
#     with open(a, "r") as file:
#         ca = {*file.read().split()}
#     with open(b, "r") as file:
#         cb = {*file.read().split()}
#     if cb & ca:
#         print(a, b,  cb & ca)
