//
//  EpochTileView.swift
//  Neural Analysis
//
//  Created by Brandon Kynoch on 2022/07/17.
//

import SwiftUI

struct EpochTileView: View {
    @EnvironmentObject var dataHandler: DataHandler
    
    @ObservedObject var epoch: EpochFolder
    
    var body: some View {
        ZStack {
            if dataHandler.currentViewingEpochFolder != epoch {
                PanelView(type: .withinWindow)
            } else {
                PanelSolidView(colour: COL_TEXT)
            }
            
            VStack {
                Text(epoch.folder.lastPathComponent)
                    .foregroundColor(COL_ACC1)
                    .modifier(TitleTextModifier())
                
                if epoch.expandedView {
                    ForEach (epoch.nns, id: \.folder) { folder in
                        VStack {
                            DirectoryTileView(targetDir: folder.folder)
                                .frame(height: 30)
                        }.frame(height: 50)
                    }
                }
            }
            .padding()
        }
        .onTapGesture {
            if epoch.nns.count == 1 {
                dataHandler.SetCurrentViewingTrainingFolder(path: epoch.nns[0].folder)
            } else {
                withAnimation {
                    epoch.expandedView.toggle()
                }
            }
        }
    }
}

//struct EpochTileView_Previews: PreviewProvider {
//    static var previews: some View {
//        EpochTileView()
//    }
//}
