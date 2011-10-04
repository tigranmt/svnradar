@echo OFF

rem Configure WinMerge.
set DIFF="%~dp0.\binary\difftool\WinMerge.exe"

set LEFT_TITLE=%3
set RIGHT_TITLE=%5
set LEFT=%6
set RIGHT=%7

%DIFF%     /e /ub /dl %LEFT_TITLE% /dr %RIGHT_TITLE%   %LEFT%  %RIGHT%
