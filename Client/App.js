import { StyleSheet, View } from 'react-native';
import React from 'react';
import MessageView from './Components/MessageView';

export default function App() {
  return (
    <View style={styles.container}>
      <View style={styles.container}>
      <MessageView></MessageView>
      </View>
     
    </View> 
  );
};

const styles = StyleSheet.create({
    container:{
      flex:3,
      flexDirection:'column'
    }
})
