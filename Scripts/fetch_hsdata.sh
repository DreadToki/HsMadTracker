#!/bin/bash

# Define the repository URL and the target directory name
REPO_URL="https://github.com/HearthSim/hsdata"
REPO_DIR="../data/hsdata"

[ -d "${REPO_DIR}/.git" ] && git -C $REPO_DIR pull || git clone $REPO_URL $REPO_DIR
