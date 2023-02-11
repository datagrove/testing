import * as yargs from 'yargs'
import dotenv from 'dotenv'

export function main() {
    dotenv.config()
    
    yargs
    .scriptName('dg')
    .usage("$0 command")
    .version('0.1')
    .command({
      command: 'hello',
      describe: 'nothing',
      handler: async parsed => {
        console.log("hello")
      },
    })
    .help()
    .demandCommand()
    .argv


}
main()