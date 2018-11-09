# REM update all submodules to use tips of master branches
git submodule foreach git pull origin master

# REM recursively update the tips of master branches
git submodule update --recursive --remote