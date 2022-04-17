import { StyleSheet, View } from 'react-native';
import React from 'react';
import MessageInput from './Components/MessageInputComponent';
import MessageView from './Components/MessageView';

export default function App() {

  return (
    <View style={styles.container}>
      <MessageView></MessageView>
         <MessageInput></MessageInput>
    </View> 
  );
};

const styles = StyleSheet.create({
    container:{
      flex:4,
    }
})
