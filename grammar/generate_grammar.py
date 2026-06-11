from typing import Iterable
from nml_src.nml.editors import extract_tables as extracted
from nml_src.nml.global_constants import const_list, misc_grf_bits


def write(fname: str, content: Iterable[str]):
    with open(fname, "w", encoding="utf8") as file:
        file.write("\n".join(content))


write("./constants.txt", sorted(
    constant
    for item in const_list
    for constant in (item[0] if isinstance(item, tuple) else item)
))

write("./features.txt", extracted.feature_names_table)

write("./properties.txt", sorted(
    extracted.properties | extracted.callbacks | extracted.layout_sprites
    | extracted.act14_vars))

write("./variables.txt", sorted(extracted.variables))

write("./misc-bits.txt", sorted(misc_grf_bits))

# {
#     feat: {
#         "purchase": {
#             p
#             for k, v in callbacks[i].items()
#             for item in (v if isinstance(v, list) else [v])
#             if (p := item.get("purchase", None)) is not None
#         }
#     }
#     for feat, i in nml_src.nml.ast.general.feature_ids.items()
# }
