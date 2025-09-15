from typing import Iterable
import nml.editors.extract_tables as extracted
import nml.global_constants as glob_consts


def write(fname: str, content: Iterable[str]):
    with open(fname, "w", encoding="utf8") as file:
        file.write("\n".join(content))


write("./constants.txt", sorted([
    constant
    for item in glob_consts.const_list
    for constant in (item[0] if isinstance(item, tuple) else item)
]))

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
