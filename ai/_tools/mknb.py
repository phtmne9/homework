import sys
from pathlib import Path

import nbformat
from nbclient import NotebookClient


def cells(text):
    out = []
    kind, buf = "code", []

    for line in text.splitlines():
        if line.startswith("# %% [markdown]"):
            out.append((kind, buf))
            kind, buf = "markdown", []
        elif line.startswith("# %%"):
            out.append((kind, buf))
            kind, buf = "code", []
        else:
            buf.append(line)

    out.append((kind, buf))

    result = []
    for k, lines in out:
        body = "\n".join(lines).strip("\n")
        if not body.strip():
            continue
        if k == "markdown":
            body = "\n".join(
                l[2:] if l.startswith("# ") else l.lstrip("#") if l.strip() == "#" else l
                for l in body.splitlines()
            )
        result.append((k, body.strip()))

    return result


def build(src, dst):
    nb = nbformat.v4.new_notebook()
    nb.metadata.kernelspec = {"name": "python3", "display_name": "Python 3", "language": "python"}

    for kind, body in cells(Path(src).read_text(encoding="utf-8")):
        if kind == "markdown":
            nb.cells.append(nbformat.v4.new_markdown_cell(body))
        else:
            nb.cells.append(nbformat.v4.new_code_cell(body))

    client = NotebookClient(nb, timeout=900, kernel_name="python3",
                            resources={"metadata": {"path": str(Path(dst).parent)}})
    client.execute()

    nbformat.write(nb, dst)
    print(f"{dst}: {len(nb.cells)} ячеек, выполнено без ошибок")


if __name__ == "__main__":
    build(sys.argv[1], sys.argv[2])
