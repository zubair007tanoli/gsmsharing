import React, { useState, useMemo } from "react";
import {
  View,
  Text,
  StyleSheet,
  Pressable,
  useColorScheme,
  Platform,
  FlatList,
  TextInput,
  Alert,
  Modal,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Ionicons, MaterialCommunityIcons } from "@expo/vector-icons";
import * as Haptics from "expo-haptics";
import Animated, { useAnimatedStyle, useSharedValue, withSpring } from "react-native-reanimated";
import Colors from "@/constants/colors";
import { useDocuments, Document, Folder } from "@/context/DocumentsContext";
import { router } from "expo-router";

function InlineAdBanner({ onDismiss }: { onDismiss: () => void }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;
  return (
    <View style={[inlineAdStyles.banner, { backgroundColor: theme.card, borderColor: theme.border }]}>
      <View style={inlineAdStyles.left}>
        <View style={[inlineAdStyles.icon, { backgroundColor: "#8B5CF6" + "18" }]}>
          <Ionicons name="rocket" size={18} color="#8B5CF6" />
        </View>
        <View>
          <View style={[inlineAdStyles.sponsoredTag, { backgroundColor: "#8B5CF6" + "18" }]}>
            <Text style={[inlineAdStyles.sponsoredText, { color: "#8B5CF6", fontFamily: "Inter_600SemiBold" }]}>
              Sponsored
            </Text>
          </View>
          <Text style={[inlineAdStyles.adTitle, { color: theme.text, fontFamily: "Inter_500Medium" }]}>
            Unlock unlimited PDF tools
          </Text>
          <Text style={[inlineAdStyles.adSub, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
            Pro plan from $4.99/month
          </Text>
        </View>
      </View>
      <View style={inlineAdStyles.right}>
        <Pressable style={[inlineAdStyles.cta, { backgroundColor: "#8B5CF6" }]}>
          <Text style={[inlineAdStyles.ctaText, { fontFamily: "Inter_600SemiBold" }]}>Try Pro</Text>
        </Pressable>
        <Pressable onPress={onDismiss} style={inlineAdStyles.closeBtn}>
          <Ionicons name="close" size={13} color={theme.textMuted} />
        </Pressable>
      </View>
    </View>
  );
}

const inlineAdStyles = StyleSheet.create({
  banner: { borderRadius: 14, borderWidth: 1, padding: 12, flexDirection: "row", alignItems: "center", justifyContent: "space-between" },
  left: { flexDirection: "row", alignItems: "center", gap: 10, flex: 1 },
  icon: { width: 38, height: 38, borderRadius: 10, alignItems: "center", justifyContent: "center" },
  sponsoredTag: { alignSelf: "flex-start", paddingHorizontal: 6, paddingVertical: 2, borderRadius: 5, marginBottom: 3 },
  sponsoredText: { fontSize: 9 },
  adTitle: { fontSize: 13, marginBottom: 1 },
  adSub: { fontSize: 11 },
  right: { flexDirection: "row", alignItems: "center", gap: 6 },
  cta: { paddingHorizontal: 12, paddingVertical: 7, borderRadius: 9 },
  ctaText: { color: "#fff", fontSize: 12 },
  closeBtn: { padding: 4 },
});

const FOLDER_COLORS = ["#2563EB", "#10B981", "#8B5CF6", "#F59E0B", "#EF4444", "#EC4899", "#06B6D4"];

function DocTypeIcon({ type, size = 20 }: { type: string; size?: number }) {
  const color =
    type === "pdf" ? "#EF4444" : type === "image" ? "#8B5CF6" : type === "word" ? "#2563EB" : "#10B981";
  const icon =
    type === "pdf" ? "file-pdf-box" : type === "image" ? "file-image" : type === "word" ? "file-word" : "file-excel";
  return <MaterialCommunityIcons name={icon as any} size={size} color={color} />;
}

function FolderCard({ folder, onPress, onLongPress }: { folder: Folder; onPress: () => void; onLongPress: () => void }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;
  const scale = useSharedValue(1);
  const style = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));
  return (
    <Pressable
      onPressIn={() => { scale.value = withSpring(0.95); Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light); }}
      onPressOut={() => { scale.value = withSpring(1); }}
      onPress={onPress}
      onLongPress={onLongPress}
    >
      <Animated.View style={[styles.folderCard, { backgroundColor: theme.card }, style]}>
        <View style={[styles.folderIcon, { backgroundColor: folder.color + "20" }]}>
          <Ionicons name="folder" size={28} color={folder.color} />
        </View>
        <Text style={[styles.folderName, { color: theme.text, fontFamily: "Inter_600SemiBold" }]} numberOfLines={1}>
          {folder.name}
        </Text>
        <Text style={[styles.folderCount, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
          {folder.documentCount} docs
        </Text>
      </Animated.View>
    </Pressable>
  );
}

function DocRow({ doc, onPress, onLongPress }: { doc: Document; onPress: () => void; onLongPress: () => void }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;
  const scale = useSharedValue(1);
  const style = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));

  const formatDate = (ts: number) => {
    const d = new Date(ts);
    return d.toLocaleDateString("en-US", { month: "short", day: "numeric", year: "numeric" });
  };

  return (
    <Pressable
      onPressIn={() => { scale.value = withSpring(0.98); }}
      onPressOut={() => { scale.value = withSpring(1); }}
      onPress={onPress}
      onLongPress={onLongPress}
    >
      <Animated.View style={[styles.docRow, { backgroundColor: theme.card }, style]}>
        <View style={[styles.docRowIcon, { backgroundColor: theme.surfaceSecondary }]}>
          <DocTypeIcon type={doc.type} size={26} />
        </View>
        <View style={styles.docRowInfo}>
          <Text style={[styles.docRowName, { color: theme.text, fontFamily: "Inter_500Medium" }]} numberOfLines={1}>
            {doc.name}
          </Text>
          <Text style={[styles.docRowMeta, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
            {doc.pages}p · {doc.size} · {formatDate(doc.updatedAt)}
          </Text>
        </View>
        {doc.starred && <Ionicons name="star" size={14} color="#F59E0B" style={{ marginRight: 4 }} />}
        <Ionicons name="ellipsis-vertical" size={18} color={theme.textMuted} />
      </Animated.View>
    </Pressable>
  );
}

export default function DocumentsScreen() {
  const colorScheme = useColorScheme();
  const isDark = colorScheme === "dark";
  const theme = isDark ? Colors.dark : Colors.light;
  const insets = useSafeAreaInsets();
  const { documents, folders, addFolder, deleteDocument, deleteFolder, toggleStar } = useDocuments();

  const [search, setSearch] = useState("");
  const [activeFolder, setActiveFolder] = useState<string | null>(null);
  const [sortBy, setSortBy] = useState<"date" | "name" | "size">("date");
  const [showNewFolder, setShowNewFolder] = useState(false);
  const [newFolderName, setNewFolderName] = useState("");
  const [newFolderColor, setNewFolderColor] = useState(FOLDER_COLORS[0]);
  const [showDocAd, setShowDocAd] = useState(true);

  const topPad = Platform.OS === "web" ? Math.max(insets.top, 67) : insets.top;

  const filteredDocs = useMemo(() => {
    let docs = activeFolder
      ? documents.filter((d) => d.folderId === activeFolder)
      : documents;
    if (search.trim()) {
      docs = docs.filter((d) => d.name.toLowerCase().includes(search.toLowerCase()));
    }
    return [...docs].sort((a, b) => {
      if (sortBy === "date") return b.updatedAt - a.updatedAt;
      if (sortBy === "name") return a.name.localeCompare(b.name);
      if (sortBy === "size") return parseFloat(b.size) - parseFloat(a.size);
      return 0;
    });
  }, [documents, search, activeFolder, sortBy]);

  const handleDocLongPress = (doc: Document) => {
    Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Medium);
    Alert.alert(doc.name, "What would you like to do?", [
      { text: doc.starred ? "Unstar" : "Star", onPress: () => toggleStar(doc.id) },
      { text: "Delete", style: "destructive", onPress: () => {
        Alert.alert("Delete Document", `Are you sure you want to delete "${doc.name}"?`, [
          { text: "Cancel" },
          { text: "Delete", style: "destructive", onPress: () => deleteDocument(doc.id) },
        ]);
      }},
      { text: "Cancel", style: "cancel" },
    ]);
  };

  const handleFolderLongPress = (folder: Folder) => {
    Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Medium);
    Alert.alert(folder.name, "Delete this folder?", [
      { text: "Cancel" },
      { text: "Delete", style: "destructive", onPress: () => deleteFolder(folder.id) },
    ]);
  };

  const createFolder = async () => {
    if (!newFolderName.trim()) return;
    await addFolder(newFolderName.trim(), newFolderColor);
    setNewFolderName("");
    setShowNewFolder(false);
    Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
  };

  return (
    <View style={[styles.container, { backgroundColor: theme.background }]}>
      {/* Header */}
      <View style={[styles.header, { paddingTop: topPad + 16 }]}>
        <Text style={[styles.title, { color: theme.text, fontFamily: "Inter_700Bold" }]}>Documents</Text>
        <Pressable
          style={[styles.newFolderBtn, { backgroundColor: theme.primary }]}
          onPress={() => setShowNewFolder(true)}
        >
          <Ionicons name="add" size={20} color="#fff" />
        </Pressable>
      </View>

      {/* Search */}
      <View style={[styles.searchBar, { backgroundColor: theme.card }]}>
        <Ionicons name="search-outline" size={18} color={theme.textMuted} />
        <TextInput
          style={[styles.searchInput, { color: theme.text, fontFamily: "Inter_400Regular" }]}
          placeholder="Search documents..."
          placeholderTextColor={theme.textMuted}
          value={search}
          onChangeText={setSearch}
        />
        {search.length > 0 && (
          <Pressable onPress={() => setSearch("")}>
            <Ionicons name="close-circle" size={18} color={theme.textMuted} />
          </Pressable>
        )}
      </View>

      {/* Folders */}
      {folders.length > 0 && !search && (
        <View style={styles.foldersSection}>
          <FlatList
            horizontal
            showsHorizontalScrollIndicator={false}
            data={folders}
            keyExtractor={(f) => f.id}
            contentContainerStyle={{ paddingHorizontal: 20, gap: 12 }}
            renderItem={({ item }) => (
              <FolderCard
                folder={item}
                onPress={() => setActiveFolder(activeFolder === item.id ? null : item.id)}
                onLongPress={() => handleFolderLongPress(item)}
              />
            )}
            ListHeaderComponent={
              <Pressable
                style={[styles.allFolderChip, { backgroundColor: !activeFolder ? theme.primary : theme.card }]}
                onPress={() => setActiveFolder(null)}
              >
                <Text style={[styles.allFolderChipText, { color: !activeFolder ? "#fff" : theme.textSecondary, fontFamily: "Inter_500Medium" }]}>
                  All
                </Text>
              </Pressable>
            }
          />
        </View>
      )}

      {/* Sort */}
      <View style={styles.sortRow}>
        <Text style={[styles.docCount, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
          {filteredDocs.length} document{filteredDocs.length !== 1 ? "s" : ""}
        </Text>
        <View style={styles.sortBtns}>
          {(["date", "name", "size"] as const).map((s) => (
            <Pressable
              key={s}
              style={[styles.sortBtn, sortBy === s && { backgroundColor: theme.primary + "20" }]}
              onPress={() => setSortBy(s)}
            >
              <Text style={[styles.sortBtnText, { color: sortBy === s ? theme.primary : theme.textMuted, fontFamily: "Inter_500Medium" }]}>
                {s.charAt(0).toUpperCase() + s.slice(1)}
              </Text>
            </Pressable>
          ))}
        </View>
      </View>

      {/* Document List */}
      <FlatList
        data={filteredDocs}
        keyExtractor={(d) => d.id}
        contentContainerStyle={{ paddingHorizontal: 16, paddingBottom: Platform.OS === "web" ? 34 : insets.bottom + 90, gap: 8, paddingTop: 4 }}
        showsVerticalScrollIndicator={false}
        renderItem={({ item, index }) => (
          <>
            <DocRow
              doc={item}
              onPress={() => router.push({ pathname: "/document/[id]", params: { id: item.id } })}
              onLongPress={() => handleDocLongPress(item)}
            />
            {index === 2 && showDocAd && (
              <InlineAdBanner onDismiss={() => setShowDocAd(false)} />
            )}
          </>
        )}
        ListEmptyComponent={
          <View style={styles.emptyState}>
            <Ionicons name="document-outline" size={52} color={theme.textMuted} />
            <Text style={[styles.emptyTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
              {search ? "No results found" : "No documents yet"}
            </Text>
            <Text style={[styles.emptySubtitle, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
              {search ? "Try a different search term" : "Scan or import your first document"}
            </Text>
          </View>
        }
      />

      {/* New Folder Modal */}
      <Modal visible={showNewFolder} transparent animationType="slide" onRequestClose={() => setShowNewFolder(false)}>
        <Pressable style={styles.modalOverlay} onPress={() => setShowNewFolder(false)}>
          <Pressable style={[styles.modalSheet, { backgroundColor: theme.surface }]} onPress={() => {}}>
            <View style={styles.modalHandle} />
            <Text style={[styles.modalTitle, { color: theme.text, fontFamily: "Inter_700Bold" }]}>
              New Folder
            </Text>
            <TextInput
              style={[styles.folderInput, { backgroundColor: theme.surfaceSecondary, color: theme.text, fontFamily: "Inter_400Regular" }]}
              placeholder="Folder name..."
              placeholderTextColor={theme.textMuted}
              value={newFolderName}
              onChangeText={setNewFolderName}
              autoFocus
            />
            <Text style={[styles.colorLabel, { color: theme.textSecondary, fontFamily: "Inter_500Medium" }]}>
              Color
            </Text>
            <View style={styles.colorRow}>
              {FOLDER_COLORS.map((c) => (
                <Pressable
                  key={c}
                  style={[styles.colorDot, { backgroundColor: c }, newFolderColor === c && styles.colorDotSelected]}
                  onPress={() => setNewFolderColor(c)}
                />
              ))}
            </View>
            <Pressable
              style={[styles.createBtn, { backgroundColor: theme.primary }]}
              onPress={createFolder}
            >
              <Text style={[styles.createBtnText, { fontFamily: "Inter_600SemiBold" }]}>Create Folder</Text>
            </Pressable>
          </Pressable>
        </Pressable>
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: { flexDirection: "row", justifyContent: "space-between", alignItems: "center", paddingHorizontal: 20, paddingBottom: 16 },
  title: { fontSize: 28 },
  newFolderBtn: { width: 36, height: 36, borderRadius: 18, alignItems: "center", justifyContent: "center" },
  searchBar: { marginHorizontal: 16, borderRadius: 14, paddingHorizontal: 14, paddingVertical: 12, flexDirection: "row", alignItems: "center", gap: 10, marginBottom: 16 },
  searchInput: { flex: 1, fontSize: 15 },
  foldersSection: { marginBottom: 14 },
  allFolderChip: { height: 88, paddingHorizontal: 20, borderRadius: 16, alignItems: "center", justifyContent: "center", minWidth: 60 },
  allFolderChipText: { fontSize: 13 },
  folderCard: { width: 90, height: 88, borderRadius: 16, alignItems: "center", justifyContent: "center", gap: 4, padding: 8 },
  folderIcon: { width: 44, height: 44, borderRadius: 12, alignItems: "center", justifyContent: "center" },
  folderName: { fontSize: 12, textAlign: "center" },
  folderCount: { fontSize: 10 },
  sortRow: { flexDirection: "row", justifyContent: "space-between", alignItems: "center", paddingHorizontal: 16, marginBottom: 8 },
  docCount: { fontSize: 13 },
  sortBtns: { flexDirection: "row", gap: 6 },
  sortBtn: { paddingHorizontal: 10, paddingVertical: 5, borderRadius: 8 },
  sortBtnText: { fontSize: 12 },
  docRow: { flexDirection: "row", alignItems: "center", borderRadius: 14, padding: 14, gap: 12 },
  docRowIcon: { width: 46, height: 46, borderRadius: 12, alignItems: "center", justifyContent: "center" },
  docRowInfo: { flex: 1 },
  docRowName: { fontSize: 14, marginBottom: 3 },
  docRowMeta: { fontSize: 12 },
  emptyState: { alignItems: "center", paddingTop: 60, gap: 10 },
  emptyTitle: { fontSize: 18 },
  emptySubtitle: { fontSize: 14, textAlign: "center", paddingHorizontal: 40 },
  modalOverlay: { flex: 1, backgroundColor: "rgba(0,0,0,0.5)", justifyContent: "flex-end" },
  modalSheet: { borderTopLeftRadius: 24, borderTopRightRadius: 24, padding: 24, paddingBottom: 40 },
  modalHandle: { width: 40, height: 4, backgroundColor: "#ccc", borderRadius: 2, alignSelf: "center", marginBottom: 20 },
  modalTitle: { fontSize: 22, marginBottom: 16 },
  folderInput: { borderRadius: 14, padding: 14, fontSize: 15, marginBottom: 20 },
  colorLabel: { fontSize: 14, marginBottom: 12 },
  colorRow: { flexDirection: "row", gap: 12, marginBottom: 24, flexWrap: "wrap" },
  colorDot: { width: 32, height: 32, borderRadius: 16 },
  colorDotSelected: { borderWidth: 3, borderColor: "#fff", shadowColor: "#000", shadowOffset: { width: 0, height: 2 }, shadowOpacity: 0.3, shadowRadius: 4 },
  createBtn: { borderRadius: 16, padding: 16, alignItems: "center" },
  createBtnText: { color: "#fff", fontSize: 16 },
});
