#!/bin/bash
# Parameter completion for the dotnet CLI

_dotnet ()
{
  local cur #current word
  local kind
  local args

  cur=${COMP_WORDS[COMP_CWORD]}
  first=${COMP_WORDS[1]}
  prev=${COMP_WORDS[COMP_CWORD - 1]}

  if [ "$prev" != "dotnet" ]; then
      all=$( /usr/local/bin/_dotnet-complete ${COMP_WORDS[@]} | grep -Po ".*" )
      kind=$( echo $all | grep -Po "\w+(?=:)" )
      args=$( echo $all | grep -Po "(?<=:).*" )

      if [ "$kind" == "WORDS" ]; then
        COMPREPLY=( $( compgen -o bashdefault -W '${args[@]}' -- ${cur} ) );
      fi

      if [ "$kind" == "DIRS" ]; then
        COMPREPLY=( $( compgen -o dirnames -- ${cur} ) );
      fi

      if [ "$kind" == "FILES" ]; then
        COMPREPLY=( $( compgen -o filenames -- ${cur} ) );
      fi
      
      return 0;
  fi

  words=("new" "restore" "build" "publish" "run" "test" "pack" "help" "-v" "--version" "--verbose" "--info")

  for p in $( echo $PATH | grep -Po "[^:]+" ); do
    if [ -d $p ]; then
      for cmd in $( ls -L1 --color=never "$p" | grep -Po "(?<=dotnet-)\w+" ); do
        words+=("$cmd")
      done
    fi
  done

  if [[ " ${words[@]} " =~ " ${prev} " ]]; then
    if [ "$prev" != "help" ]; then
      return 0;
    fi
  fi

  COMPREPLY=( $( compgen -W '${words[@]}' -- $cur ) );
}

complete -o default -F _dotnet dotnet