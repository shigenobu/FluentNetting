<?php
$data = msgpack_unpack(file_get_contents('FluentNest/data.bin'));
var_dump($data);

echo "+++++++++++++++++++++++++++++++++++\n";

$unpacker = new \MessagePackUnpacker(false);
$buffer = $data[1];
$nread = 0;

while(true) {
   if($unpacker->execute($buffer, $nread)) {
       $msg = $unpacker->data();
       
       var_dump($msg);
       
       $unpacker->reset();
       $buffer = substr($buffer, $nread);
       $nread = 0;
       if(!empty($buffer)) {
            continue;
       }
   }
   break;
}